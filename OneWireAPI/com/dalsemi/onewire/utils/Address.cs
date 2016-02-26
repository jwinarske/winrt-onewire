﻿/*---------------------------------------------------------------------------
 * Copyright (C) 1999,2004 Dallas Semiconductor Corporation, All Rights Reserved.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included
 * in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
 * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY,  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL DALLAS SEMICONDUCTOR BE LIABLE FOR ANY CLAIM, DAMAGES
 * OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
 * ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 *
 * Except as contained in this notice, the name of Dallas Semiconductor
 * shall not be used except as stated in the Dallas Semiconductor
 * Branding Policy.
 *---------------------------------------------------------------------------
 */

namespace com.dalsemi.onewire.utils
{

	/// <summary>
	/// Utilities to translate and verify the 1-Wire Network address.
	/// <para>
	/// </para>
	/// Q: What is a 1-Wire Network Address?<para>
	/// A: A 1-Wire address is 64 bits consisting of an eight bit family code, forty eight
	/// bits of serialized data and an eight bit CRC8 of the first 56 bits.
	/// </para>
	/// <para>
	/// For example given the following address in hexadecimal:
	/// </para>
	/// <para>
	/// 10 28 E9 14 00 00 00 F3
	/// </para>
	/// <para>
	/// The above is a family code 10 device with a serialized data
	/// of 28 E9 14 00 00 00, and a CRC8 of F3.
	/// </para>
	/// <para>
	/// The address can be stored in several ways:
	/// <ul>
	/// <li>
	/// </para>
	/// As a little-endian byte array:<para>
	/// </para>
	/// <code>byte[] address = { 0x10, (byte)0xE9, 0x14, 0x00, 0x00, 0x00, (byte)0xF3 };</code><para>
	/// </li>
	/// <li>
	/// </para>
	/// As a big-endian long:<para>
	/// </para>
	/// <code>long address = (long)0xF300000014E92810;</code><para>
	/// </li>
	/// <li>
	/// </para>
	/// As a big-endian String:<para>
	/// </para>
	/// <code>String address = "F300000014E92810";</code><para>
	/// </li>
	/// </ul>
	/// @version    0.00, 21 August 2000
	/// @author     DS
	/// </para>
	/// </summary>
	public class Address
	{

	   //--------
	   //-------- Constructor
	   //--------

	   /// <summary>
	   /// Private constructor to prevent instantiation.
	   /// </summary>
	   private Address()
	   {
	   }

	   //--------
	   //-------- Methods
	   //--------

	   /// <summary>
	   /// Checks the CRC8 calculation of this 1-Wire Network address.
	   /// <para>
	   /// The address is valid if the CRC8 of the first seven bytes of the address gives
	   /// a result equal to the eighth byte.
	   /// 
	   /// </para>
	   /// </summary>
	   /// <param name="address">  iButton or 1-Wire Network address to verify
	   /// </param>
	   /// <returns> <code>true</code> if the family code is non-zero and the
	   /// CRC8 calculation is correct. </returns>
	   /// <seealso cref=        com.dalsemi.onewire.utils.CRC8 </seealso>
	   public static bool isValid(sbyte[] address)
	   {
		  if ((address [0] != 0) && (CRC8.compute(address) == 0))
		  {
			 return true;
		  }
		  else if ((address[0] & 0x7F) == 0x1C) // DS28E04
		  {
			 // The DS28E04 has a pin selectable ROM ID input.  However,
			 // the CRC8 for the ROM ID assumes that the selecatable bits
			 // are always 1.
			 return 0 == CRC8.compute(address, 2, 6, CRC8.compute(0x7F, CRC8.compute(address[0], 0)));
		  }
		  else
		  {
			 return false;
		  }
	   }

	   /// <summary>
	   /// Checks the CRC8 calculation of this 1-Wire Network address.
	   /// <para>
	   /// The address is valid if the CRC8 of the first seven bytes of the address gives
	   /// a result equal to the eighth byte.
	   /// 
	   /// </para>
	   /// </summary>
	   /// <param name="address">  iButton or 1-Wire Network address to verify
	   /// </param>
	   /// <returns> <code>true</code> if the family code is non-zero and the
	   /// CRC8 calculation is correct. </returns>
	   /// <seealso cref=        com.dalsemi.onewire.utils.CRC8 </seealso>
	   public static bool isValid(string address)
	   {
		  return isValid(toByteArray(address));
	   }

	   /// <summary>
	   /// Checks the CRC8 calculation of this 1-Wire Network address.
	   /// <para>
	   /// The address is valid if the CRC8 of the first seven bytes of the address gives
	   /// a result equal to the eighth byte.
	   /// 
	   /// </para>
	   /// </summary>
	   /// <param name="address">  iButton or 1-Wire Network address to verify
	   /// </param>
	   /// <returns> <code>true</code> if the family code is non-zero and the
	   /// CRC8 calculation is correct. </returns>
	   /// <seealso cref=        com.dalsemi.onewire.utils.CRC8 </seealso>
	   public static bool isValid(long address)
	   {
		  return isValid(toByteArray(address));
	   }

	   /// <summary>
	   /// Converts a 1-Wire Network address byte array (little endian)
	   /// to a hex string representation (big endian).
	   /// </summary>
	   /// <param name="address"> family code first.
	   /// </param>
	   /// <returns> address represented in a String, family code last. </returns>
	   public static string ToString(sbyte[] address)
	   {
		  // When displaying, the CRC is first, family code is last so
		  // that the center 6 bytes are a real serial number (not byte reversed).

		  sbyte[] barr = new sbyte[16];
		  int index = 0;
		  int ch;

		  for (int i = 7;i >= 0;i--)
		  {
			ch = (address[i] >> 4) & 0x0F;
			ch += ((ch > 9) ? 'A' - 10 : '0');
			barr[index++] = (sbyte)ch;
			ch = address[i] & 0x0F;
			ch += ((ch > 9) ? 'A' - 10 : '0');
			barr[index++] = (sbyte)ch;
		  }

		  return StringHelperClass.NewString(barr);
	   }

	   /// <summary>
	   /// Converts a 1-Wire Network address long (little endian)
	   /// to a hex string representation (big endian).
	   /// </summary>
	   /// <param name="address"> family code first.
	   /// </param>
	   /// <returns> address represented in a long, little endian. </returns>
	   public static string ToString(long address)
	   {
		  return ToString(toByteArray(address));
	   }

	   /// <summary>
	   /// Converts a 1-Wire Network Address string (big endian)
	   /// to a byte array (little endian).
	   /// </summary>
	   /// <param name="address"> family code last.
	   /// </param>
	   /// <returns> address represented in a byte array, family
	   ///                 code (LS byte) first. </returns>
	   public static sbyte[] toByteArray(string address)
	   {
		  sbyte[] address_byte = new sbyte [8];

		  for (int i = 0; i < 8; i++)
		  {
			 address_byte [7 - i] = (sbyte)((Character.digit((address[i * 2]), 16) << 4) | (Character.digit(address[i * 2 + 1], 16)));
		  }

		  return address_byte;
	   }

	   /// <summary>
	   /// Convert an iButton or 1-Wire device address as a long
	   /// (little endian) into an array of bytes.
	   /// </summary>
	   public static sbyte[] toByteArray(long address)
	   {

		  /* This looks funny, but it should actually take
		     less time since I do 7 eight bit shifts instead
		     of 8+16+24+32+40+48+56 shifts.
		  */
		  sbyte[] address_byte = new sbyte [8];

		  address_byte [0] = (sbyte) address;
		  address = (long)((ulong)address >> 8);
		  address_byte [1] = (sbyte) address;
		  address = (long)((ulong)address >> 8);
		  address_byte [2] = (sbyte) address;
		  address = (long)((ulong)address >> 8);
		  address_byte [3] = (sbyte) address;
		  address = (long)((ulong)address >> 8);
		  address_byte [4] = (sbyte) address;
		  address = (long)((ulong)address >> 8);
		  address_byte [5] = (sbyte) address;
		  address = (long)((ulong)address >> 8);
		  address_byte [6] = (sbyte) address;
		  address = (long)((ulong)address >> 8);
		  address_byte [7] = (sbyte) address;

		  return address_byte;
	   }

	   /// <summary>
	   /// Converts a 1-Wire Network Address to a long (little endian).
	   /// </summary>
	   /// <returns> address represented as a long. </returns>
	   public static long toLong(sbyte[] address)
	   {
		  /* This looks funny, but it should actually take
		     less time since I do 7 eight bit shifts instead
		     of 8+16+24+32+40+48+56 shifts.
		  */
		  long longVal = (long)(address [7] & 0xFF);
		  longVal <<= 8;
		  longVal |= (long)(address [6] & 0xFF);
		  longVal <<= 8;
		  longVal |= (long)(address [5] & 0xFF);
		  longVal <<= 8;
		  longVal |= (long)(address [4] & 0xFF);
		  longVal <<= 8;
		  longVal |= (long)(address [3] & 0xFF);
		  longVal <<= 8;
		  longVal |= (long)(address [2] & 0xFF);
		  longVal <<= 8;
		  longVal |= (long)(address [1] & 0xFF);
		  longVal <<= 8;
		  longVal |= (long)(address [0] & 0xFF);

		  return longVal;
	   }

	   /// <summary>
	   /// Converts a 1-Wire Network Address to a long (little endian).
	   /// </summary>
	   /// <returns> address represented as a String. </returns>
	   public static long toLong(string address)
	   {
		  return toLong(toByteArray(address));
	   }
	}

}