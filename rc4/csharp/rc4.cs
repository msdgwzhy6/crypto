using System;
using System.IO;

class RC4Stream : Stream
{
	private Stream stream;
	private uint[] s = new uint[256];
	private byte i, j;

	public RC4Stream(Stream stream, byte[] key) {
		this.stream = stream;

		int k = key.Length;
		if (k < 1 || k > 256) {
			throw new KeySizeException(k);
		}

		for (uint i = 0; i < 256; i++) {
			s[i] = i;
		}

		byte j = 0;
		uint t = 0;
		for (int i = 0; i < 256; i++) {
			j += (byte)((byte)s[i] + key[i % k]);
			t = s[i];
			s[i] = s[j];
			s[j] = t;
		}
	}

	private void xorKeyStreamGeneric(byte[] dst, int dstOffset, byte[] src, int srcOffset, int count) {
		byte i = this.i;
		byte j = this.j;
		uint t = 0;
		for (int k = 0; k < count; k ++) {
			i += 1;
			j += (byte)s[i];
			t = s[i];
			s[i] = s[j];
			s[j] = t;
			dst[k + dstOffset] = (byte)(src[k + srcOffset] ^ (byte)(s[(byte)(s[i] + s[j])]));
		}
		this.i = i;
		this.j = j;
	}

	public class KeySizeException : Exception
	{
		private int size;

		public KeySizeException(int size) {
			this.size = size;
		}

		public override string Message {
			get { return "RC4Stream: invalid key size " + size; }
		}
	}

	public override int Read(byte[] buffer, int offset, int count) {
		count = stream.Read(buffer, offset, count);
		xorKeyStreamGeneric(buffer, offset, buffer, offset, count);
		return count;
	}

	public override void Write(byte[] buffer, int offset, int count) {
		byte[] dst = new byte[count];
		xorKeyStreamGeneric(dst, 0, buffer, offset, count);
		stream.Write(dst, 0, count);
	}

	public override bool CanRead {
		get { return stream.CanRead; }
	}

	public override bool CanSeek {
		get { return stream.CanSeek; }
	}

	public override bool CanWrite {
		get { return stream.CanWrite; }
	}

	public override long Length {
		get { return stream.Length; }
	}

	public override long Position {
		get { return stream.Position; }
		set { stream.Position = value; }
	}

	public override long Seek(long offset, SeekOrigin origin) {
		return stream.Seek(offset, origin);
	}

	public override void SetLength(long length) {
		stream.SetLength(length);
	}

	public override void Flush() {
		stream.Flush();
	}
}