// Guids.cs
// MUST match guids.h
using System;

namespace VS.Menu
{
    static class GuidList
    {
        public const string guidVSMenuControlPkgString = "9e7ff0e2-ba79-49aa-8ab5-4a22c6213f5e";

        public static readonly Guid guidVSMenuControlCmdSet4All = new Guid("842cb5a4-f55f-45a1-bbd4-c43f4a35163d");

        // Thrift
        public static readonly Guid guidVSMenuContolCmdSet4Thrift = new Guid("66425cfc-8a56-4bb1-a0c5-98683364aa7d");

        // Grpc
        public static readonly Guid guidVSMenuControlCmdSet4Grpc = new Guid("6fe93241-a429-4491-bab2-f53490511ed6");

    };
}