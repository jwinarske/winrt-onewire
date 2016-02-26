﻿using System;

/*---------------------------------------------------------------------------
 * Copyright (C) 1999,2000 Dallas Semiconductor Corporation, All Rights Reserved.
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

namespace com.dalsemi.onewire
{

	// imports


	/// <summary>
	/// This is the general exception thrown by the iButton and 1-Wire
	/// operations.
	/// 
	/// @version    0.00, 21 August 2000
	/// @author     DS
	/// </summary>
	public class OneWireException : Exception
	{

	   //--------
	   //-------- Contructor
	   //--------

	   /// <summary>
	   /// Constructs a <code>OneWireException</code> with no detail message.
	   /// </summary>
	   public OneWireException() : base()
	   {
	   }

	   /// <summary>
	   /// Constructs a <code>OneWireException</code> with the specified detail message.
	   /// </summary>
	   /// <param name="desc">   the detail message description </param>
	   public OneWireException(string desc) : base(desc)
	   {
	   }
	}

}