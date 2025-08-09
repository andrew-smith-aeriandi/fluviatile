using Fluviatile.Grid;
using Fluviatile.Grid.Random;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;

namespace SvgGenerator;

public class Program
{
    public static async Task Main(string[] args)
    {
        //Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

        var seedArgIndex = Array.IndexOf<string>(args, "-r") + 1;
        var jsonArgIndex = Array.IndexOf<string>(args, "-s") + 1;
        var pathArgIndex = Array.IndexOf<string>(args, "-f") + 1;
        var countArgIndex = Array.IndexOf<string>(args, "-c") + 1;

        var seed = seedArgIndex > 0 &&
            args.Length > seedArgIndex &&
            int.TryParse(args[seedArgIndex], out var arg)
                ? arg
                : Environment.TickCount;

        var random = new Pseudorandom(seed);

        const int size = 3;
        var shape = new Hexagon(size);
        var routeFinder = new RouteFinder(random, shape);

        var grid = new HexGrid(size, 0.3d, random);

        var pathJson = (string?)null;
        if (jsonArgIndex > 0 && args.Length > jsonArgIndex)
        {
            pathJson = args[jsonArgIndex];
        }
        else if (pathArgIndex > 0 && args.Length > pathArgIndex && File.Exists(args[pathArgIndex]))
        {
            using var textReader = File.OpenText(args[pathArgIndex]);
            pathJson = textReader.ReadToEnd();
        }

        var countJson = (string?)null;
        if (countArgIndex > 0 && args.Length > countArgIndex)
        {
            countJson = args[countArgIndex];
        }

        var nodeCounts = (IReadOnlyList<int>?)null;

        if (!string.IsNullOrEmpty(pathJson))
        {
            var path = JsonSerializer.Deserialize<List<Coordinates>>(pathJson);
            if (path is not null)
            {
                grid.SetSequence(path.Select(coord => (coord.X, coord.Y)));
            }
        }
        else if (!string.IsNullOrEmpty(countJson))
        {
            nodeCounts = JsonSerializer.Deserialize<List<int>>(countJson);
            if (nodeCounts is not null)
            {
                grid.SetNodeCounts(NodeCountHelper.MapNodeCounts(nodeCounts));
            }
        }
        else
        {
            await routeFinder.Initiate(Configuration.NodeCountsFilename(shape));
            nodeCounts = routeFinder.SelectRandomNodeCount();

            if (nodeCounts is not null)
            {
                grid.SetNodeCounts(NodeCountHelper.MapNodeCounts(nodeCounts));
            }
            else
            {
                grid.CreateSequence();
            }
        }

        var outputPath = @"C:\git\Scratch\Fluviatile\Results\Test.html";
        var fileStream = new StreamWriter(outputPath, false, Encoding.UTF8);
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            OmitXmlDeclaration = true
        };

        using var xmlWriter = XmlWriter.Create(fileStream, settings);

        xmlWriter.WriteDocType("html", null, null, null);
        xmlWriter.WriteStartElement("html");
        xmlWriter.WriteAttributeString("lang", "en");
        xmlWriter.WriteStartElement("head");
        xmlWriter.WriteStartElement("meta");
        xmlWriter.WriteAttributeString("charset", "utf-8");
        xmlWriter.WriteFullEndElement(); // meta
        xmlWriter.WriteStartElement("meta");
        xmlWriter.WriteAttributeString("name", "viewport");
        xmlWriter.WriteAttributeString("content", "width=device-width, initial-scale=1");
        xmlWriter.WriteFullEndElement(); // meta
        xmlWriter.WriteElementString("title", "Fluviatile");
        xmlWriter.WriteStartElement("style");
        xmlWriter.WriteRaw(GetCss());
        xmlWriter.WriteFullEndElement(); // style
        xmlWriter.WriteStartElement("script");
        xmlWriter.WriteRaw(GetScript());
        xmlWriter.WriteFullEndElement(); // script
        xmlWriter.WriteFullEndElement(); // head
        xmlWriter.WriteStartElement("body");

        var svgWriter = new SvgWriter(xmlWriter);
        svgWriter.WriteSvg(grid);

        xmlWriter.WriteFullEndElement(); // body
        xmlWriter.WriteFullEndElement(); // html

        xmlWriter.Flush();

        Console.WriteLine($"Output written to:\n{outputPath}");
        Console.WriteLine($"Node Counts: [{string.Join(", ", NodeCountHelper.MapNodeCountsForSolver(nodeCounts))}]");
        Console.ReadLine();
    }

    private static string GetCss()
    {
        return @"
svg text {
   -webkit-user-select: none;
   -moz-user-select: none;
   -ms-user-select: none;
   user-select: none;
}

svg text::selection {
   background: none;
}

#fluviatile-grid {
   cursor: pointer;
}

#fluviatile-grid.tracing {
   cursor: grabbing;
}

.margin {
   pointer-events: none;
   fill: LightSteelBlue;
   fill-opacity: 1.0;
   stroke: White;
   stroke-width: 0;
   stroke-linejoin: round;
}

.margin.line {
   stroke-width: 2;
}

.button {
   cursor: default;
   font: bold 24px sans-serif;
   fill: SlateGray;
}

.button:hover {
   fill: DarkSlateGray;
}

.node-count {
   pointer-events: none;
   font: bold 24px sans-serif;
   fill: White;
}

.node-count.error {
   fill: Red;
}

.cell {
   fill: LightGreen;
   fill-opacity: 1.0;
   stroke: Linen;
   stroke-width: 2;
   stroke-linejoin: round;
}

.cell.plain {
   fill: ForestGreen;
}

.river {
   pointer-events: none;
   fill: none;
   stroke: Teal;
   stroke-width: 3;
   stroke-linejoin: round;
}

.trace {
   pointer-events: none;
   fill: none;
   stroke: Teal;
   stroke-width: 1;
   stroke-dasharray: 2;
}
";
    }

    private static string GetScript()
    {
        return @"
const Fluviatile = function() {
   this.svg = undefined;
   this.tracedChannel = [];
   this.mouseButtons = 0;
   this.gridCells = {};
   this.counts = {x: {}, y: {}, z: {}};
   this.lastTracedCell = null;
   this.savedState = [];
   this.dirty = false;
}

Fluviatile.prototype.initiate = function(svgId) {
   var svg = document.getElementById(svgId);
   this.svg = svg;

   svg.addEventListener('mousedown', mouseDownHandler.bind(this));
   svg.addEventListener('mousemove', mouseMoveHandler.bind(this));
   svg.addEventListener('mouseup', mouseUpHandler.bind(this));

   var saveStateButton = document.getElementById('save-state');
   saveStateButton.addEventListener('click', saveStateHandler.bind(this));

   var revertStateButton = document.getElementById('revert-state');
   revertStateButton.addEventListener('click', revertStateHandler.bind(this));

   const cells = svg.querySelectorAll('.cell');
   const state = {};

   for (let i = 0; i < cells.length; i++) {
      const cell = cells[i];
      const id = cell.getAttribute('id');

      const gridCell = createCell(
         0,
         parseInt(cell.getAttribute('data-u'), 10),
         parseInt(cell.getAttribute('data-v'), 10),
         parseFloat(cell.getAttribute('data-x')),
         parseFloat(cell.getAttribute('data-y')));

      this.gridCells[id] = gridCell;
      state[id] = gridCell.state;

      cell.addEventListener('contextmenu', contextMenuHandler.bind(this));
      cell.addEventListener('mousedown', mouseDownHandler.bind(this));
      cell.addEventListener('mousemove', mouseMoveHandler.bind(this));
      cell.addEventListener('mouseenter', mouseEnterHandler.bind(this));
      cell.addEventListener('mouseup', mouseUpHandler.bind(this));
   }

   this.savedState.push(state);
   this.dirty = false;

   const nodeCounts = svg.querySelectorAll('.node-count');

   for (let i = 0; i < nodeCounts.length; i++) {
      const nodeCount = nodeCounts[i];
      const group = nodeCount.getAttribute('data-group');
      const index = nodeCount.getAttribute('data-index');
      this.counts[group][index] = {
         id: nodeCount.getAttribute('id'),
         count: parseInt(nodeCount.getAttribute('data-count'), 10),
         max: parseInt(nodeCount.getAttribute('data-max'), 10),
         positive: 0,
         negative: 0
      };
   }
};

function saveStateHandler(e) {
   this.saveState();
}

function revertStateHandler(e) {
   this.revertState();
}

function contextMenuHandler(e) {
   e.preventDefault();
   return false;
}

function mouseDownHandler(e) {
   e.stopPropagation();
   this.mouseButtons = e.buttons;

   const cell = this.gridCells[e.target.id];
   if ((this.mouseButtons & 1) === 1 && (cell === undefined || (cell.state & 1) === 1)) {
      this.lastTracedCell = cell || createCell(0);
   }
}

function mouseMoveHandler(e) {
   e.stopPropagation();
   if (!this.lastTracedCell) return;

   if (this.tracedChannel.length === 0) {
      this.startChannelTrace();
      return;
   }

   const previous = this.lastTracedCell;
   if (previous.state === 0) return;

   let cell = this.gridCells[e.target.id];
   if (cell !== undefined) return;

   let state = 0;
   if (previous.u === 1) {
      state = 3;
      cell = createCell(0, previous.u - 2, previous.v - 1);
   } else if (previous.u === 17) {
      state = 17;
      cell = createCell(0, previous.u + 2, previous.v + 1);
   } else if (previous.v === 1) {
      state = 65;
      cell = createCell(0, previous.u - 1, previous.v - 2);
   } else if (previous.v === 17) {
      state = 9;
      cell = createCell(0, previous.u + 1, previous.v + 2);
   } else if (previous.u - previous.v === 8) {
      state = 5;
      cell = createCell(0, previous.u + 1, previous.v - 1);
   } else if (previous.v - previous.u === 8) {
      state = 33;
      cell = createCell(0, previous.u - 1, previous.v + 1);
   } else {
     this.cancelChannelTrace();
     return;
   }
   
   const combinedState = previous.state | state;
   if (combinedState === 15 || combinedState === 113) {
      this.cancelChannelTrace();
      return;
   }

   if (combinedState !== previous.state) {
      this.tracedChannel.push(cell);
      this.addElement(previous, 'channel', state, 'trace');
   }
}

function mouseEnterHandler(e) {
   e.stopPropagation();
   if (this.tracedChannel.length === 0) return;

   const cell = this.gridCells[e.target.id];
   if (cell === undefined || cell === this.lastTracedCell || (cell.state & 1) === 0 || this.tracedChannel.indexOf(cell) >= 0) {
      this.cancelChannelTrace();
      return;
   }

   const previous = this.lastTracedCell;
   if (previous.state !== 0 && Math.abs(cell.u - previous.u) + Math.abs(cell.v - previous.v) > 3) {
      this.cancelChannelTrace();
      return;
   }

   if (previous.state === 0) {
      if (cell.u === 1) {
         previous.u = cell.u - 2;
         previous.v = cell.v - 1;
      } else if (cell.u === 17) {
         previous.u = cell.u + 2;
         previous.v = cell.v + 1;
      } else if (cell.v === 1) {
         previous.u = cell.u - 1;
         previous.v = cell.v - 2;
      } else if (cell.v === 17) {
         previous.u = cell.u + 1;
         previous.v = cell.v + 2
      } else if (cell.u - cell.v === 8) {
         previous.u = cell.u + 1;
         previous.v = cell.v - 1;
      } else if (cell.v - cell.u === 8) {
         previous.u = cell.u - 1;
         previous.v = cell.v + 1;
      } else {
         this.cancelChannelTrace();
         return;
      }
   }

   const u = cell.u - previous.u;
   const v = cell.v - previous.v;
   let state1 = 0;
   let state2 = 0;

   if (u === 1 && v === -1) {
      state1 = 5;
      state2 = 33;
   } else if (u === -1 && v === 1) {
      state1 = 33; 
      state2 = 5;
   } else if (u === 1 && v === 2) {
      state1 = 9;
      state2 = 65;
   } else if (u === -1 && v === -2) {
      state1 = 65;
      state2 = 9;
   } else if (u === -2 && v === -1) {
      state1 = 3;
      state2 = 17;
   } else if (u === 2 && v === 1) {
      state1 = 17;
      state2 = 3;
   }

   const combinedState1 = previous.state | state1;
   const combinedState2 = cell.state | state2;

   if (combinedState1 === 15 || combinedState1 === 113 || combinedState2 === 15 || combinedState2 === 113) {
      this.cancelChannelTrace();
      return;
   }

   if (combinedState1 !== previous.state) {
      this.addElement(previous, 'channel', state1, 'trace');
   }

   if (combinedState2 !== cell.state) {
      this.addElement(cell, 'channel', state2, 'trace');
   }

   this.tracedChannel.push(cell);
   this.lastTracedCell = cell;
}

function mouseUpHandler(e) {
   e.stopPropagation();
   const cell = this.gridCells[e.target.id];
   const mouseButtons = this.mouseButtons;
   this.mouseButtons = 0;

   if (this.tracedChannel.length > 1) {
      this.completeChannelTrace();
      return;
   }

   this.cancelChannelTrace();

   if (cell) {
      xcount = this.counts['x'][String(Math.floor(cell.u / 3))];
      ycount = this.counts['y'][String(Math.floor(cell.v / 3))];
      zcount = this.counts['z'][String(Math.floor((cell.v - cell.u) / 3 + 3))];

      if ((mouseButtons & 1) === 1) {
         if (cell.state === 0) {
            xcount.positive += 1;
            ycount.positive += 1;
            zcount.positive += 1;
            cell.state = 1;
            this.dirty = true;
            this.addElement(cell, 'channel', 1);
         } else if ((cell.state & 1) === 1) {
            xcount.positive -= 1;
            ycount.positive -= 1;
            zcount.positive -= 1;
            cell.state = 0;
            this.dirty = true;
            this.clearElements(cell);
         }
      } else if ((mouseButtons & 2) === 2) {
         if (cell.state === 0) {
            xcount.negative += 1;
            ycount.negative += 1;
            zcount.negative += 1;
            cell.state = 256;
            this.dirty = true;
            e.target.classList.add('plain');
         } else if ((cell.state & 256) === 256) {
            xcount.negative -= 1;
            ycount.negative -= 1;
            zcount.negative -= 1;
            cell.state = 0;
            this.dirty = true;
            e.target.classList.remove('plain');
         }
      }

      const xnodeCount = document.getElementById(xcount.id);
      if (xcount.positive > xcount.count || xcount.negative > xcount.max - xcount.count) {
         xnodeCount.classList.add('error');
      } else {
         xnodeCount.classList.remove('error');
      }

      const ynodeCount = document.getElementById(ycount.id);
      if (ycount.positive > ycount.count || ycount.negative > ycount.max - ycount.count) {
         ynodeCount.classList.add('error');
      } else {
         ynodeCount.classList.remove('error');
      }

      const znodeCount = document.getElementById(zcount.id);
      if (zcount.positive > zcount.count || zcount.negative > zcount.max - zcount.count) {
         znodeCount.classList.add('error');
      } else {
         znodeCount.classList.remove('error');
      }
   }
}

Fluviatile.prototype.startChannelTrace = function() {
   if (!this.lastTracedCell) return;

   this.svg.classList.add('tracing');
   this.tracedChannel.length = 0;
   this.tracedChannel.push(this.lastTracedCell);
};

Fluviatile.prototype.cancelChannelTrace = function() {
   this.svg.classList.remove('tracing');

   let cell, elementId;
   while (cell = this.tracedChannel.pop()) {
      const updatedElements = [];
      while (elementId = cell.elements.pop()) {
         const element = document.getElementById(elementId);
         if (element) {
            if (element.classList.contains('trace')) {
               element.parentNode.removeChild(element);
            } else {
               updatedElements.push(elementId);
            }
         }
      }
      cell.elements = updatedElements;
   }

   this.lastTracedCell = null;
   this.mouseButtons = 0;
}

Fluviatile.prototype.completeChannelTrace = function() {
   this.svg.classList.remove('tracing');

   for (let i = 0; i < this.tracedChannel.length; i++) {
      const before = i - 1 >= 0 ? this.tracedChannel[i - 1] : null;
      const cell = this.tracedChannel[i];
      const after = i + 1 < this.tracedChannel.length ? this.tracedChannel[i + 1] : null;
      let state = 0;

      if (before !== null && after !== null) {
         const u1 = cell.u - before.u;
         const v1 = cell.v - before.v;
         const u2 = after.u - cell.u;
         const v2 = after.v - cell.v;

         if ((u1 === -1 && v1 === 1 && u2 === 1 && v2 === 2) || (u1 === -1 && v1 === -2 && u2 === 1 && v2 === -1)) {
            state = 13;
         } else if ((u1 === 1 && v1 === 2 && u2 === 2 && v2 === 1) || (u1 === -2 && v1 === -1 && u2 === -1 && v2 === -2)) {
            state = 81;
         } else if ((u1 === 2 && v1 === 1 && u2 === 1 && v2 === -1) || (u1 === -1 && v1 === 1 && u2 === -2 && v2 === -1)) {
            state = 7;
         } else if ((u1 === 1 && v1 === -1 && u2 === -1 && v2 === -2) || (u1 === 1 && v1 === 2 && u2 === -1 && v2 === 1)) {
            state = 97;
         } else if ((u1 === -1 && v1 === -2 && u2 === -2 && v2 === -1) || (u1 === 2 && v1 === 1 && u2 === 1 && v2 === 2)) {
            state = 11;
         } else if ((u1 === -2 && v1 === -1 && u2 === -1 && v2 === 1) || (u1 === 1 && v1 === -1 && u2 === 2 && v2 === 1)) {
            state = 49;
         }
      } else {
         const u = before !== null ? before.u - cell.u : after.u - cell.u;
         const v = before !== null ? before.v - cell.v : after.v - cell.v;

         if (u === -1 && v === -2) {
            state = 65;
         } else if (u === 1 && v === -1) {
            state = 5;
         } else if (u === 2 && v === 1) {
            state = 17;
         } else if (u === 1 && v === 2) {
            state = 9;
         } else if (u === -1 && v === 1) {
            state = 33;
         } else if (u === -2 && v === -1) {
            state = 3;
         }
      }

      const desiredState = state | cell.state;
      if (desiredState !== cell.state) {
         cell.state = desiredState;
         this.dirty = true;
         this.clearElements(cell);
         this.addElement(cell, 'channel', desiredState);
      }
   }

   this.tracedChannel.length = 0;
   this.lastTracedCell = null;
}

Fluviatile.prototype.addElement = function(cell, typePrefix, typeId, className) {
   if (!cell || cell.x === undefined || cell.x === null || cell.y === undefined || cell.y === null) return;

   const elementId = getId(typePrefix);
   const href = '#' + typePrefix + (!typeId ? '' : '-' + typeId);

   const element = document.createElementNS('http://www.w3.org/2000/svg', 'use');
   element.setAttribute('href', href);
   element.setAttribute('id', elementId);
   element.setAttribute('x', cell.x);
   element.setAttribute('y', cell.y);

   if (className) {
      element.setAttribute('class', className);
   }

   this.svg.appendChild(element);
   cell.elements.push(elementId);
}

Fluviatile.prototype.clearElements = function(cell) {
   let elementId;

   while (elementId = cell.elements.pop()) {
      const element = document.getElementById(elementId);
      if (element) {
         element.parentNode.removeChild(element);
      }
   }
}

Fluviatile.prototype.saveState = function() {
   this.cancelChannelTrace();
   if (!this.dirty) return;

   const state = {};
   for (let id in this.gridCells) {
      state[id] = this.gridCells[id].state;
   }

   this.savedState.push(state);
   this.dirty = false;
};

Fluviatile.prototype.revertState = function() {
   this.cancelChannelTrace();

   const state = !this.dirty && this.savedState.length > 1
      ? this.savedState.pop()
      : this.savedState[this.savedState.length - 1];

   this.dirty = false;

   for (let group in this.counts) {
      for (let index in this.counts[group]) {
         const count = this.counts[group][index];
         count.positive = 0;
         count.negative = 0;
      }
   }

   for (let id in this.gridCells) {
      const cell = this.gridCells[id];
      cell.state = state[id];
      this.clearElements(cell);

      const element = document.getElementById(id);
      element.classList.remove('plain');

      if (cell.state === 0) {
         continue;
      }

      const xcount = this.counts['x'][String(Math.floor(cell.u / 3))];
      const ycount = this.counts['y'][String(Math.floor(cell.v / 3))];
      const zcount = this.counts['z'][String(Math.floor((cell.v - cell.u) / 3 + 3))];

      if ((cell.state & 1) === 1) {
         element.classList.remove('plain');
         this.addElement(cell, 'channel', cell.state);
         xcount.positive += 1;
         ycount.positive += 1;
         zcount.positive += 1;
      } else if ((cell.state & 256) === 256) {
         element.classList.add('plain');
         xcount.negative += 1;
         ycount.negative += 1;
         zcount.negative += 1;
      }
   }

   for (let group in this.counts) {
      for (let index in this.counts[group]) {
         const count = this.counts[group][index];
         const nodeCount = document.getElementById(count.id);
         if (count.positive > count.count || count.negative > count.max - count.count) {
            nodeCount.classList.add('error');
         } else {
            nodeCount.classList.remove('error');
         }
      }
   }
};

const createCell = function(state, u, v, x, y) {
   return {
      state: typeof state === 'number' ? state : 0,
      u: typeof u === 'number' ? u : null,
      v: typeof v === 'number' ? v : null,
      x: typeof x === 'number' ? x : null,
      y: typeof y === 'number' ? y : null,
      elements: []
   };
};

const getId = (function() {
   let prefixes = {};
   return function(prefix) {
      const value = (prefixes[prefix] || 0) + 1;
      prefixes[prefix] = value;
      return prefix + '_' + value;
   };
})();

const fluviatile = new Fluviatile();

document.addEventListener('DOMContentLoaded', function() {
   fluviatile.initiate('fluviatile-grid');
   fluviatile.saveState();
}, false);

// Inject polyfill if classList not supported for SVG elements.
// https://github.com/tobua/svg-classlist-polyfill/blob/master/polyfill.js
if (!('classList' in SVGElement.prototype)) {
  Object.defineProperty(SVGElement.prototype, 'classList', {
    get: function get() {
      var _this = this

      return {
        contains: function contains(className) {
          return _this.className.baseVal.split(' ').indexOf(className) !== -1
        },
        add: function add(className) {
          var newClass = (_this.getAttribute('class') + ' ' + className).trim()
          return _this.setAttribute('class', newClass)
        },
        remove: function remove(className) {
          var classes = _this.getAttribute('class') || ''
          var regex = new RegExp('(?:^|\\s)' + className + '(?!\\S)', 'g')
          classes = classes.replace(regex, '').trim()
          _this.setAttribute('class', classes)
        },
        toggle: function toggle(className) {
          if (this.contains(className)) {
            this.remove(className)
          } else {
            this.add(className)
          }
        },
      }
    },
  })
}
";
    }
}
