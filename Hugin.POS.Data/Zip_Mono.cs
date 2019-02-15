
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Hugin.POS.Data
{
	class Zip_Mono
	{
	}
	class ZipEntry
	{
		DateTime datetime = DateTime.Now;
		String name = String.Empty;
		public ZipEntry (String path)
		{
			throw new Exception("Zip not supported");
		}
		public DateTime DateTime {
			get{ return datetime;}
			set{ datetime = value;}
		}
		public String Name {
			get{ return name;}
			set{ name = value;}
		}
	}
	class ZipOutputStream
	{
		public ZipOutputStream (System.IO.Stream file )
		{
			throw new Exception("Zip not supported");
		}
		public void SetLevel (int level)
		{
			throw new Exception("function is not supported");
		}
		public void PutNextEntry (ZipEntry entry)
		{
			throw new Exception("function is not supported");
		}
		public void Write (byte [] buffer, int offset, int numOfBytes)
		{
			throw new Exception("function is not supported");
		}
		public void Finish ()
		{

		}		
		public void Close ()
		{
			
		}
	}
	class ZipInputStream
	{
		public ZipInputStream (System.IO.Stream file )
		{
			throw new Exception("Zip not supported");
		}

		public ZipEntry GetNextEntry ()
		{
			throw new Exception("function is not supported");
		}
		public int Read (byte [] buffer, int offset, int numOfBytes)
		{
			throw new Exception("function is not supported");
		}
		public void Finish ()
		{
			
		}		
		public void Close ()
		{
			
		}
	}
}

