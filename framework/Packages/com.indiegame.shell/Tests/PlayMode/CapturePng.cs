using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;
namespace IndieGame.Shell.Tests
{
    // The smoke project omits Unity's ImageConversion module. Use the PNG standard
    // (RGB8 scanlines, filter 0, zlib/Deflate) without changing its package manifest.
    internal static class CapturePng
    {
        static void UInt(Stream stream,uint value) { for(int shift=24;shift>=0;shift-=8) stream.WriteByte((byte)(value>>shift)); }
        static void Chunk(Stream output,string kind,byte[] data)
        {
            UInt(output,(uint)data.Length); var type=Encoding.ASCII.GetBytes(kind); output.Write(type,0,4); output.Write(data,0,data.Length);
            uint crc=0xffffffff;
            foreach(var bytes in new[]{type,data}) foreach(byte b in bytes) { crc^=b; for(int k=0;k<8;k++) crc=(crc&1)!=0?0xedb88320^(crc>>1):crc>>1; }
            UInt(output,crc^0xffffffff);
        }
        internal static byte[] Encode(Texture2D texture)
        {
            var colors=texture.GetPixels32(); var scanlines=new byte[texture.height*(1+texture.width*3)]; int offset=0;
            for(int y=texture.height-1;y>=0;y--) { scanlines[offset++]=0; for(int x=0;x<texture.width;x++) { var c=colors[y*texture.width+x]; scanlines[offset++]=c.r; scanlines[offset++]=c.g; scanlines[offset++]=c.b; } }
            using var output=new MemoryStream(); output.Write(new byte[]{137,80,78,71,13,10,26,10},0,8);
            using(var header=new MemoryStream()) { UInt(header,(uint)texture.width); UInt(header,(uint)texture.height); header.Write(new byte[]{8,2,0,0,0},0,5); Chunk(output,"IHDR",header.ToArray()); }
            using(var zlib=new MemoryStream())
            {
                zlib.WriteByte(0x78); zlib.WriteByte(0x9c);
                using(var deflate=new DeflateStream(zlib,System.IO.Compression.CompressionLevel.Optimal,true)) deflate.Write(scanlines,0,scanlines.Length);
                uint a=1,b=0; foreach(byte c in scanlines) { a=(a+c)%65521; b=(b+a)%65521; } UInt(zlib,(b<<16)|a);
                Chunk(output,"IDAT",zlib.ToArray());
            }
            Chunk(output,"IEND",Array.Empty<byte>()); return output.ToArray();
        }
    }
}
