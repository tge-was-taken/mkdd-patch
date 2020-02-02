#region License
/*
Copied & adapted from https://github.com/arookas/flaaffy/blob/master/mareep/waveform.cs

MIT License

Copyright( c) 2017 

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion
using System;
using System.Linq;
using System.Text;

namespace arookas
{
	public static class Waveform
	{

		private static int[] sSigned2BitTable = new int[4] {
			0, 1, -2, -1,
		};
		private static int[] sSigned4BitTable = new int[16] {
			0, 1, 2, 3, 4, 5, 6, 7, -8, -7, -6, -5, -4, -3, -2, -1,
		};
		private static short[,] sAdpcmCoefficents = new short[16, 2] {
			{ 0, 0, }, { 2048, 0, }, { 0, 2048, }, { 1024, 1024, },
			{ 4096, -2048, }, { 3584, -1536, }, { 3072, -1024, }, { 4608, -2560, },
			{ 4200, -2248, }, { 4800, -2300, }, { 5120, -3072, }, { 2048, -2048, },
			{ 1024, -1024, }, { -1024, 1024, }, { -1024, 0, }, { -2048, 0, },
		};

		private static short ClampSample16Bit( int sample )
		{
			if ( sample < -32768 )
			{
				sample = -32768;
			}
			else if ( sample > 32767 )
			{
				sample = 32767;
			}

			return ( short )sample;
		}

		public static void Adpcm4ToPcm8( byte[] adpcm4, sbyte[] pcm8, ref int last, ref int penult )
		{
			var pcm16 = new short[16];
			Adpcm4ToPcm16( adpcm4, pcm16, ref last, ref penult );
			for ( var i = 0; i < 16; ++i )
			{
				Pcm16ToPcm8( pcm16[i], out pcm8[i] );
			}
		}
		public static void Adpcm4ToPcm16( byte[] adpcm4, short[] pcm16, ref int last, ref int penult )
		{
			var header = adpcm4[0];
			var nibbleCoeff = (2048 << (header >> 4));

			var coeffIndex = (header & 0xF);
			var lastCoeff = sAdpcmCoefficents[coeffIndex, 0];
			var penultCoeff = sAdpcmCoefficents[coeffIndex, 1];

			for ( var i = 0; i < 8; ++i )
			{
				var input = adpcm4[1 + i];

				for ( var j = 0; j < 2; ++j )
				{
					var nibble = sSigned4BitTable[(input >> 4) & 0xF];
					var sample = ClampSample16Bit((nibbleCoeff * nibble + lastCoeff * last + penultCoeff * penult) >> 11);

					penult = last;
					last = sample;
					pcm16[i * 2 + j] = sample;
					input <<= 4;
				}
			}
		}
		public static void Pcm8ToAdpcm4( sbyte[] pcm8, byte[] adpcm4, ref int last, ref int penult )
		{
			var pcm16 = new short[16];

			for ( var i = 0; i < 16; ++i )
			{
				Pcm8ToPcm16( pcm8[i], out pcm16[i] );
			}

			Pcm16ToAdpcm4( pcm16, adpcm4, ref last, ref penult );
		}
		public static void Pcm16ToAdpcm4( short[] pcm16, byte[] adpcm4, ref int last, ref int penult )
		{
			// check if all samples in frame are zero
			// if so, write out an empty adpcm frame
			if ( pcm16.All( sample => sample == 0 ) )
			{
				for ( var i = 0; i < 9; ++i )
				{
					adpcm4[i] = 0;
				}

				last = 0;
				penult = 0;

				return;
			}

			var pcm4 = false;
			var nibbles = new int[16];
			int coeffIndex = 0, scale = 0;

			// try to use coefficient zero for static silence
			for ( var i = 0; i < 3; ++i )
			{
				var step = (1 << i);
				var range = (8 << i);

				if ( pcm16.All( sample => sample >= -range && sample < range ) )
				{
					pcm4 = true;
					coeffIndex = 0;
					scale = i;
					break;
				}
			}

			if ( !pcm4 )
			{
				coeffIndex = -1;
				var minerror = Int32.MaxValue;

				// otherwise, select one of the remaining coefficients by smallest error
				for ( var coeff = 1; coeff < 16; ++coeff )
				{
					var lastCoeff = sAdpcmCoefficents[coeff, 0];
					var penultCoeff = sAdpcmCoefficents[coeff, 1];
					var found_scale = -1;
					var coeff_error = 0;

					// select the first scale that fits all differences
					for ( var current_scale = 0; current_scale < 16; ++current_scale )
					{
						var step = (1 << current_scale);
						var nibbleCoeff = (2048 << current_scale);
						var success = true;
						coeff_error = 0;

						// use non-ref copies
						var _last = last;
						var _penult = penult;

						for ( var i = 0; i < 16; ++i )
						{
							var prediction = ClampSample16Bit((lastCoeff * _last + penultCoeff * _penult) >> 11);
							var difference = -(prediction - pcm16[i]); // negate because we need to counteract it
							var nibble = (difference / step);

							if ( nibble < -8 || nibble > 7 )
							{
								success = false;
								break;
							}

							var decoded = ClampSample16Bit((nibbleCoeff * nibble + lastCoeff * _last + penultCoeff * _penult) >> 11);

							_penult = _last;
							_last = decoded;

							// don't let +/- differences cancel each other out
							coeff_error += System.Math.Abs( difference );
						}

						if ( success )
						{
							found_scale = current_scale;
							break;
						}
					}

					if ( found_scale < 0 )
					{
						continue;
					}

					if ( coeff_error < minerror )
					{
						minerror = coeff_error;
						coeffIndex = coeff;
						scale = found_scale;
					}
				}

				if ( coeffIndex < 0 )
				{
					var sb = new StringBuilder(256);
					sb.Append( "could not find coefficient!\nPCM16:" );

					for ( var i = 0; i < 16; ++i )
					{
						sb.AppendFormat( " {0,6}", pcm16[i] );
					}

					sb.AppendFormat( "\nLAST: {0,6} PENULT: {1,6}\n", last, penult );

					//mareep.WriteError( sb.ToString() );
				}
			}

			{
				// calculate each delta and write to the nibbles
				var lastCoeff = sAdpcmCoefficents[coeffIndex, 0];
				var penultCoeff = sAdpcmCoefficents[coeffIndex, 1];

				var step = (1 << scale);

				for ( var i = 0; i < 16; ++i )
				{
					var prediction = ClampSample16Bit((lastCoeff * last + penultCoeff * penult) >> 11);
					var difference = -(prediction - pcm16[i]); // negate because we need to counteract it
					nibbles[i] = ( difference / step );

					var decoded = ClampSample16Bit((nibbles[i] * (2048 << scale) + lastCoeff * last + penultCoeff * penult) >> 11);

					penult = last;
					last = decoded;
				}
			}

			// write out adpcm bytes
			adpcm4[0] = ( byte )( ( scale << 4 ) | coeffIndex );

			for ( var i = 0; i < 8; ++i )
			{
				adpcm4[1 + i] = ( byte )( ( ( nibbles[i * 2] << 4 ) & 0xF0 ) | ( nibbles[i * 2 + 1] & 0xF ) );
			}
		}

		public static void Adpcm2ToPcm8( byte[] adpcm2, sbyte[] pcm8, ref int last, ref int penult )
		{
			var pcm16 = new short[16];
			Adpcm2ToPcm16( adpcm2, pcm16, ref last, ref penult );
			for ( var i = 0; i < 16; ++i )
			{
				Pcm16ToPcm8( pcm16[i], out pcm8[i] );
			}
		}
		public static void Adpcm2ToPcm16( byte[] adpcm2, short[] pcm16, ref int last, ref int penult )
		{
			var header = adpcm2[0];
			var nibbleCoeff = (8192 << (header >> 4));

			var coeffIndex = (header & 0xF);
			var lastCoeff = sAdpcmCoefficents[coeffIndex, 0];
			var penultCoeff = sAdpcmCoefficents[coeffIndex, 1];

			for ( var i = 0; i < 4; ++i )
			{
				var input = adpcm2[1 + i];

				for ( var j = 0; j < 4; ++j )
				{
					var nibble = sSigned2BitTable[(input >> 6) & 0x3];
					var sample = ClampSample16Bit(((nibble * nibbleCoeff) + (lastCoeff * last) + (penultCoeff * penult)) >> 11);

					penult = last;
					last = sample;
					pcm16[i * 4 + j] = sample;
					input <<= 2;
				}
			}
		}
		public static void Pcm8ToAdpcm2( sbyte[] pcm8, byte[] adpcm2, ref int last, ref int penult )
		{
			var pcm16 = new short[16];

			for ( var i = 0; i < 16; ++i )
			{
				Pcm8ToPcm16( pcm8[i], out pcm16[i] );
			}

			Pcm16ToAdpcm2( pcm16, adpcm2, ref last, ref penult );
		}
		public static void Pcm16ToAdpcm2( short[] pcm16, byte[] adpcm2, ref int last, ref int penult )
		{
			// check if all samples in frame are zero
			// if so, write out an empty adpcm frame
			if ( pcm16.All( sample => sample == 0 ) )
			{
				for ( var i = 0; i < 5; ++i )
				{
					adpcm2[i] = 0;
				}

				last = 0;
				penult = 0;

				return;
			}

			var pcm4 = false;
			var nibbles = new int[16];
			int coeffIndex = 0, scale = 0;

			// try to use coefficient zero for static silence
			for ( var i = 0; i < 2; ++i )
			{
				var step = (1 << i);
				var range = (2 << i);

				if ( pcm16.All( sample => sample >= -range && sample < range ) )
				{
					pcm4 = true;
					coeffIndex = 0;
					scale = i;
					break;
				}
			}

			if ( !pcm4 )
			{
				coeffIndex = -1;
				var minerror = Int32.MaxValue;

				// otherwise, select one of the remaining coefficients by smallest error
				for ( var coeff = 1; coeff < 16; ++coeff )
				{
					var lastCoeff = sAdpcmCoefficents[coeff, 0];
					var penultCoeff = sAdpcmCoefficents[coeff, 1];
					var found_scale = -1;
					var coeff_error = 0;

					// select the first scale that fits all differences
					for ( var current_scale = 0; current_scale < 16; ++current_scale )
					{
						var step = (1 << current_scale);
						var nibbleCoeff = (8192 << current_scale);
						var success = true;
						coeff_error = 0;

						// use non-ref copies
						var _last = last;
						var _penult = penult;

						for ( var i = 0; i < 16; ++i )
						{
							var prediction = ClampSample16Bit((lastCoeff * _last + penultCoeff * _penult) >> 11);
							var difference = -(prediction - pcm16[i]); // negate because we need to counteract it
							var nibble = (difference / step);

							if ( nibble < -2 || nibble > 1 )
							{
								success = false;
								break;
							}

							var decoded = ClampSample16Bit((nibbleCoeff * nibble + lastCoeff * _last + penultCoeff * _penult) >> 11);

							_penult = _last;
							_last = decoded;

							// don't let +/- differences cancel each other out
							coeff_error += System.Math.Abs( difference );
						}

						if ( success )
						{
							found_scale = current_scale;
							break;
						}
					}

					if ( found_scale < 0 )
					{
						continue;
					}

					if ( coeff_error < minerror )
					{
						minerror = coeff_error;
						coeffIndex = coeff;
						scale = found_scale;
					}
				}

				if ( coeffIndex < 0 )
				{
					var sb = new StringBuilder(256);
					sb.Append( "could not find coefficient!\nPCM16:" );

					for ( var i = 0; i < 16; ++i )
					{
						sb.AppendFormat( " {0,6}", pcm16[i] );
					}

					sb.AppendFormat( "\nLAST: {0,6} PENULT: {1,6}\n", last, penult );

					//mareep.WriteError( sb.ToString() );
				}
			}

			{
				// calculate each delta and write to the nibbles
				var lastCoeff = sAdpcmCoefficents[coeffIndex, 0];
				var penultCoeff = sAdpcmCoefficents[coeffIndex, 1];

				var step = (1 << scale);

				for ( var i = 0; i < 16; ++i )
				{
					var prediction = ClampSample16Bit((lastCoeff * last + penultCoeff * penult) / 2048);
					var difference = -(prediction - pcm16[i]); // negate because we need to counteract it
					nibbles[i] = ( difference / step );

					var decoded = ClampSample16Bit((nibbles[i] * (8192 << scale) + lastCoeff * last + penultCoeff * penult) / 2048);

					penult = last;
					last = decoded;
				}
			}

			// write out adpcm bytes
			adpcm2[0] = ( byte )( ( scale << 4 ) | coeffIndex );

			for ( var i = 0; i < 4; ++i )
			{
				adpcm2[1 + i] = ( byte )( ( ( nibbles[i * 2] << 4 ) & 0xC0 ) | ( ( nibbles[i * 2 + 1] << 4 ) & 0x30 ) | ( ( nibbles[i * 2 + 2] << 4 ) & 0x0C ) | ( nibbles[i * 2 + 3] & 0x03 ) );
			}
		}

		public static void Pcm8ToPcm16( sbyte pcm8, out short pcm16 )
		{
			pcm16 = ( short )( pcm8 * ( pcm8 < 0 ? 256 : 258 ) );
		}
		public static void Pcm16ToPcm8( short pcm16, out sbyte pcm8 )
		{
			pcm8 = ( sbyte )( pcm16 >> 8 );
		}

		public static void Pcm32ToPcm16( float pcm32, out short pcm16 )
		{
			pcm16 = ( short )( pcm32 * 32767 );
		}

		public static short[] Pcm32ToPcm16( float[] pcm32 )
		{
			var pcm16 = new short[pcm32.Length];
			for ( int i = 0; i < pcm16.Length; i++ )
			{
				Pcm32ToPcm16( pcm32[i], out var pcm16Sample );
				pcm16[i] = pcm16Sample;
			}

			return pcm16;
		}

		public static byte[] Pcm16ToAdpcm4( short[] pcm16 )
		{
			return Pcm16ToAdpcm4( pcm16, null );
		}

		public static byte[] Pcm16ToAdpcm4( short[] pcm16, AdpcmHistory[] historyTable )
		{
			var output = new byte[(GetAdpcmFrameCount(pcm16.Length) * 9)];
			var outputIdx = 0;
			var adpcm4Frame = new byte[9];
			var pcm16Frame = new short[16];
			int last = 0, penult = 0;

			for ( var i = 0; i < pcm16.Length; i += 16 )
			{
				for ( var j = 0; j < 16; ++j )
				{
					pcm16Frame[j] = 0;

					if ( i + j < pcm16.Length )
					{
						pcm16Frame[j] = pcm16[i + j];
					}
				}

				Pcm16ToAdpcm4( pcm16Frame, adpcm4Frame, ref last, ref penult );

				if ( historyTable != null )
					historyTable[i / 16] = new AdpcmHistory() { Last = last, Penult = penult };

				Array.Copy( adpcm4Frame, 0, output, outputIdx, adpcm4Frame.Length );
				outputIdx += adpcm4Frame.Length;
			}

			return output;
		}

		public static void CalculateHistoryAdpcm2( short[] pcm16, int sample, out int last, out int penult )
		{
			last = 0;
			penult = 0;

			var frame = (sample / 16);

			if ( frame == 0 )
			{
				return;
			}

			var adpcm2Frame = new byte[5];
			var pcm16Frame = new short[16];

			int _last = 0, _penult = 0;

			for ( var i = 0; i < frame; ++i )
			{
				for ( var j = 0; j < 16; ++j )
				{
					pcm16Frame[j] = 0;

					if ( ( i * 16 + j ) < pcm16.Length )
					{
						pcm16Frame[j] = pcm16[i * 16 + j];
					}
				}

				Pcm16ToAdpcm2( pcm16Frame, adpcm2Frame, ref _last, ref _penult );
				Adpcm2ToPcm16( adpcm2Frame, pcm16Frame, ref last, ref penult );
			}
		}

		public static void CalculateHistoryAdpcm4( short[] pcm16, int sample, out int last, out int penult )
		{
			last = 0;
			penult = 0;

			var frame = (sample / 16);

			if ( frame == 0 )
			{
				return;
			}

			var adpcm4Frame = new byte[9];
			var pcm16Frame = new short[16];

			int _last = 0, _penult = 0;

			for ( var i = 0; i < frame; ++i )
			{
				for ( var j = 0; j < 16; ++j )
				{
					pcm16Frame[j] = 0;

					if ( ( i * 16 + j ) < pcm16.Length )
					{
						pcm16Frame[j] = pcm16[i * 16 + j];
					}
				}

				Pcm16ToAdpcm4( pcm16, adpcm4Frame, ref _last, ref _penult );
				Adpcm4ToPcm16( adpcm4Frame, pcm16, ref last, ref penult );
			}
		}

		public static int GetAdpcmFrameCount( int pcmSampleCount )
		{
			return ( pcmSampleCount / 16 ) + 1;
		}
	}

	public struct AdpcmHistory
	{
		public int Last;
		public int Penult;
	}
}