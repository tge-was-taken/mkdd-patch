using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace MKDD.Patcher.IO
{
    public enum IOMode
    {
        Read,
        Write
    }

    public enum Endianness
    {
        Little,
        Big
    }

    public enum Storage
    {
        ByValue,
        ByReference
    }

    public enum Origin
    {
        Begin,
        Current,
        End,
        OffsetBase
    }

    public class SeekToken : IDisposable
    {
        public BinaryIOStream Stream { get; }
        public long OldPosition { get; }
        public long NewPosition { get; }

        public SeekToken( BinaryIOStream stream, long position )
        {
            Stream = stream;
            NewPosition = position;
            OldPosition = stream.Position;

            Stream.Position = NewPosition;
        }

        public void Dispose()
        {
            Stream.Position = OldPosition;
        }
    }

    public class BinaryIOStream : IDisposable
    {
        private bool mLeaveOpen;
        private byte[] mBuffer = new byte[4096];
        private byte[] mZeroBuffer = new byte[4096];

        public Stream BaseStream { get; private set; }
        public Encoding Encoding { get; private set; }
        public IOMode Mode { get; set; }
        public Endianness Endianness { get; set; }
        public long Position
        {
            get => BaseStream.Position;
            set => BaseStream.Position = value;
        }
        public Stack<long> OffsetBase { get; set; }

        public List<long> OffsetPositions { get; }

        public BinaryIOStream( Stream input, IOMode mode, Endianness endianness )
        {
            Initialize( input, mode, endianness, Encoding.Default, leaveOpen: false );
        }

        public BinaryIOStream( Stream input, IOMode mode, Endianness endianness, Encoding encoding )
        {
            Initialize( input, mode, endianness, encoding, leaveOpen: false );
        }

        public BinaryIOStream( Stream input, IOMode mode, Endianness endianness, Encoding encoding, bool leaveOpen )
        {
            Initialize( input, mode, endianness, encoding, leaveOpen );
        }

        public void PushOffsetBase()
        {
            OffsetBase.Push( Position );
        }

        public long PopOffsetBase()
        {
            return OffsetBase.Pop();
        }

        public sbyte ReadSByte()
        {
            return ( sbyte )ReadByte();
        }

        public void WriteSByte( sbyte v )
        {
            WriteByte( ( byte )v );
        }

        public byte ReadByte()
        {
            return ( byte )BaseStream.ReadByte();
        }

        public void WriteByte( byte value )
        {
            BaseStream.WriteByte( value );
        }

        public short ReadInt16()
        {
            BaseStream.Read( mBuffer, 0, sizeof( short ) );
            var value = BitConverter.ToInt16( mBuffer, 0 );
            if ( Endianness != EndiannessConverter.NativeEndianness ) value = EndiannessConverter.Swap( value );
            return value;
        }

        public void WriteInt16( short value )
        {
            if ( Endianness != EndiannessConverter.NativeEndianness ) value = EndiannessConverter.Swap( value );
            var bytes = BitConverter.GetBytes( value );
            BaseStream.Write( bytes, 0, bytes.Length );
        }

        public ushort ReadUInt16() => ( ushort )ReadInt16();

        public void WriteUInt16( ushort value ) => WriteInt16( ( short )value );

        public int ReadInt32()
        {
            BaseStream.Read( mBuffer, 0, sizeof( int ) );
            var value = BitConverter.ToInt32( mBuffer, 0 );
            if ( Endianness != EndiannessConverter.NativeEndianness ) value = EndiannessConverter.Swap( value );
            return value;
        }

        public void WriteInt32( int value )
        {
            if ( Endianness != EndiannessConverter.NativeEndianness ) value = EndiannessConverter.Swap( value );
            var bytes = BitConverter.GetBytes( value );
            BaseStream.Write( bytes, 0, bytes.Length );
        }

        public uint ReadUInt32()
        {
            return ( uint )ReadInt32();
        }

        public void WriteUInt32( uint value )
        {
            WriteInt32( ( int )value );
        }

        public float ReadSingle()
        {
            var value = ReadUInt32();
            return Unsafe.As<uint, float>( ref value );
        }

        public void WriteSingle( float value )
        {
            var integerValue = Unsafe.As<float, uint>(ref value);
            WriteUInt32( integerValue );
        }

        public byte[] ReadBytes(int count)
        {
            var bytes = new byte[count];
            ReadBytes( bytes, 0, count );
            return bytes;
        }

        public void ReadBytes( byte[] value, int offset, int count )
        {
            BaseStream.Read( value, offset, count );
        }

        public void WriteBytes( byte[] value, int offset, int count )
        {
            BaseStream.Write( value, offset, count );
        }

        public void WriteBytes( byte[] value )
        {
            BaseStream.Write( value, 0, value.Length );
        }

        public void Byte( ref byte value )
        {
            if ( Mode == IOMode.Read )
                value = ReadByte();
            else
                WriteByte( value );
        }

        public void Int16( ref short value )
        {
            if ( Mode == IOMode.Read )
                value = ReadInt16();
            else
                WriteInt16( value );
        }

        public void UInt16( ref ushort value )
        {
            if ( Mode == IOMode.Read )
                value = ReadUInt16();
            else
                WriteUInt16( value );
        }

        public void Int32( ref int value )
        {
            if ( Mode == IOMode.Read )
                value = ReadInt32();
            else
                WriteInt32( value );
        }

        public void UInt32( ref uint value )
        {
            if ( Mode == IOMode.Read )
                value = ReadUInt32();
            else
                WriteUInt32( value );
        }

        public void Single( ref float value )
        {
            if ( Mode == IOMode.Read )
            {
                uint temp = 0;
                UInt32( ref temp );
                value = Unsafe.As<uint, float>( ref temp );
            }
            else
            {
                var temp = Unsafe.As<float, uint>( ref value );
                UInt32( ref temp );
            }
        }

        public void Bytes( byte[] value, int offset, int count )
        {
            if ( Mode == IOMode.Read )
            {
                ReadBytes( value, offset, count );
            }
            else
            {
                WriteBytes( value, offset, count );
            }
        }

        public void String( ref string value, Storage storage, int length = -1 )
        {
            if ( Mode == IOMode.Read )
            {
                value = ReadString( storage, length );
            }
            else
            {
                WriteString( value, storage, length );
            }
        }

        public void WriteString( string value, Storage storage, int length )
        {
            if ( storage == Storage.ByValue )
            {
                WriteStringValue( value );
            }
            else
            {
                throw new NotImplementedException();
                //EnqueueRefWriteItem( new RefActionWriteQueueItem( Position, ( stream ) =>
                //{
                //    WriteStringValue( value );
                //} ) );
            }
        }

        private void WriteStringValue( string value )
        {
            var bytes = Encoding.GetBytes(value);
            WriteBytes( bytes, 0, bytes.Length );
            WriteByte( 0 );
        }

        public string ReadString( Storage storage, int length = -1 )
        {
            if ( storage == Storage.ByValue )
            {
                return ReadStringValue( length );
            }
            else
            {
                var offset = ReadUInt32();
                using ( At( offset, Origin.OffsetBase ) )
                    return ReadStringValue( length );
            }
        }

        private string ReadStringValue( int length = -1 )
        {
            var value = string.Empty;

            if ( length != -1 )
            {
                ReadBytes( mBuffer, 0, length );
            }
            else
            {
                var str = string.Empty;

                int i;
                byte b;
                for ( i = 0, b = ReadByte(); b != 0; i++ )
                    mBuffer[i] = b;

                length = i;
            }

            value = Encoding.GetString( mBuffer, 0, length )
                .TrimEnd( '\0' );

            return value;
        }

        private void Initialize( Stream input, IOMode mode, Endianness endianness, Encoding encoding, bool leaveOpen )
        {
            BaseStream = input;
            Mode = mode;
            Endianness = endianness;
            Encoding = encoding;
            mLeaveOpen = leaveOpen;
            OffsetBase = new Stack<long>();
            OffsetBase.Push( 0 );
        }

        //private void EnqueueRefWriteItem( RefWriteQueueItem item )
        //{
        //    mIOQueue.Enqueue( item );
        //    OffsetPositions.Add( item.OffsetPosition );
        //}

        public long Skip( int offset )
        {
            return BaseStream.Seek( offset, System.IO.SeekOrigin.Current ) - offset;
        }

        public long Seek( long offset, Origin origin )
        {
            if ( origin == Origin.OffsetBase )
            {
                return BaseStream.Seek( OffsetBase.Peek() + offset, System.IO.SeekOrigin.Begin );
            }
            else
            {
                return BaseStream.Seek( offset, ( System.IO.SeekOrigin )origin );
            }           
        }

        public BinaryIOStream CreateView( long offset, long length )
        {
            return new BinaryIOStream( new StreamView( BaseStream, offset, length ), Mode, Endianness, Encoding );
        }

        public SeekToken At( long offset, Origin origin )
        {
            var position = CalculatePosition( offset, origin );
            return new SeekToken( this, position );
        }

        private long CalculatePosition( long offset, Origin origin )
        {
            long position = 0;

            switch ( origin )
            {
                case Origin.Begin:
                    position = offset;
                    break;
                case Origin.Current:
                    position = Position + offset;
                    break;
                case Origin.End:
                    position = BaseStream.Length - offset;
                    break;
                case Origin.OffsetBase:
                    position = OffsetBase.Peek() + offset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException( nameof( origin ) );
            }

            return position;
        }

        public void Dispose()
        {
            if ( !mLeaveOpen )
            {
                BaseStream.Dispose();
            }
        }

        public sbyte[] ReadSBytes( int count )
        {
            var values = new sbyte[count];
            for ( int i = 0; i < values.Length; i++ )
            {
                values[i] = ReadSByte();
            }

            return values;
        }

        public void Align( int alignment )
        {
            var alignedDiff = AlignmentHelper.GetAlignedDifference(Position, 32);
            if ( Mode == IOMode.Read )
            {
                Skip( alignedDiff );
            }
            else
            {
                WriteBytes( mZeroBuffer, 0, alignedDiff );
            }
        }

        public short[] ReadInt16s( int count )
        {
            var values = new short[count];
            for ( int i = 0; i < values.Length; i++ )
                values[i] = ReadInt16();

            return values;
        }

        public void WriteSBytes( sbyte[] values )
        {
            for ( int i = 0; i < values.Length; i++ )
                WriteSByte( values[i] );
        }

        public void WriteSBytes( sbyte[] values, int v )
        {
            for ( int i = 0; i < v; i++ )
                WriteSByte( values[i] );
        }

        public void WriteInt16s( short[] v )
        {
            for ( int i = 0; i < v.Length; i++ )
                WriteInt16( v[i] );
        }

        public void WriteInt16s( short[] values, int count )
        {
            for ( int i = 0; i < count; i++ )
                WriteInt16( values[i] );
        }
    }
}
