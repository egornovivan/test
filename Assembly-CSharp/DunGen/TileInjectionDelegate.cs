using System;
using System.Collections.Generic;

namespace DunGen;

public delegate void TileInjectionDelegate(Random randomStream, ref List<InjectedTile> tilesToInject);
