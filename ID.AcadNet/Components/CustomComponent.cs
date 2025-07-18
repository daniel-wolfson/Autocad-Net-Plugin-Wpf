using System;
using System.Drawing;
using System.IO;

namespace Intellidesk.AcadNet.Components
{
    internal static class ThumbnailReader
    {
        internal static Bitmap GetBitmap(string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    fs.Seek(0xD, SeekOrigin.Begin);
                    fs.Seek(0x14 + br.ReadInt32(), SeekOrigin.Begin);
                    byte bytCnt = br.ReadByte();
                    if (bytCnt <= 1)
                        return null;
                    int imageHeaderStart;
                    int imageHeaderSize;
                    byte imageCode;
                    for (short i = 1; i <= bytCnt; i++)
                    {
                        imageCode = br.ReadByte();
                        imageHeaderStart = br.ReadInt32();
                        imageHeaderSize = br.ReadInt32();
                        if (imageCode == 2)
                        {
                            fs.Seek(imageHeaderStart, SeekOrigin.Begin);
                            //-----------------------------------------------------
                            // BITMAPINFOHEADER (40 bytes)
                            br.ReadBytes(0xE); //biSize, biWidth, biHeight, biPlanes
                            ushort biBitCount = br.ReadUInt16();
                            br.ReadBytes(4); //biCompression
                            uint biSizeImage = br.ReadUInt32();
                            //br.ReadBytes(0x10); //biXPelsPerMeter, biYPelsPerMeter, biClrUsed, biClrImportant
                            //-----------------------------------------------------
                            fs.Seek(imageHeaderStart, SeekOrigin.Begin);
                            byte[] bitmapBuffer = br.ReadBytes(imageHeaderSize);
                            uint colorTableSize = (uint)((biBitCount < 9) ? 4 * Math.Pow(2, biBitCount) : 0);
                            using (MemoryStream ms = new MemoryStream())
                            {
                                using (BinaryWriter bw = new BinaryWriter(ms))
                                {
                                    //bw.Write(new byte[] { 0x42, 0x4D });
                                    bw.Write((ushort)0x4D42);
                                    bw.Write(54U + colorTableSize + biSizeImage);
                                    bw.Write(new ushort());
                                    bw.Write(new ushort());
                                    bw.Write(54U + colorTableSize);
                                    bw.Write(bitmapBuffer);
                                    return new Bitmap(ms);
                                }
                            }
                        }
                        else if (imageCode == 3)
                        {
                            return null;
                        }
                    }
                }
            }
            return null;
        }
    }
}