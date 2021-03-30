// PkgCmdID.cs
// MUST match PkgCmdID.h
using System;

namespace VS.Menu
{
    static class PkgCmdIDList
    {
        // Thrift
        public const uint ThriftGen4Async = 0x1111;
        public const uint ThriftGen4AsyncOld = 0x1112;
        public const uint ThriftGen4Origin = 0x1113;

        public const uint ThriftGen4AsyncDll = 0x1121;

        public const uint ThriftGen4AsyncClientDll = 0x1131;
        public const uint ThriftGen4AsyncOldClientDll = 0x1132;

        public const uint ThriftGen4AsyncClientNuget = 0x1141;
        public const uint ThriftGen4AsyncOldClientNuget = 0x1142;




        // Grpc
        public const uint GrpcGen4Server = 0x2111;

        public const uint GrpcGen4Client = 0x2121;
        public const uint GrpcGen4ClientDll = 0x2122;
        public const uint GrpcGen4ClientNuget = 0x2123;
    };
}