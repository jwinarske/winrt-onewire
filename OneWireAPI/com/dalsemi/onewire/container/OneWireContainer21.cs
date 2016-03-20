﻿using System;
using System.Collections;
using System.Collections.Generic;

// C#
// Month     -> value between 1 - 12
// Day       -> value between 1 - 31
// Year      -> value between 1 - 9999
// Hour      -> value between 0 - 23
// Minute    -> value between 0 - 59
// Second    -> value between 0 - 59
// DayOfWeek -> if cast to an integer, value ranges between 0 - 6

// Java
// months are 0 offset
// years are from 1900

/*---------------------------------------------------------------------------
 * Copyright (C) 1999 - 2001 Dallas Semiconductor Corporation, All Rights Reserved.
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

namespace com.dalsemi.onewire.container
{

	using Convert = com.dalsemi.onewire.utils.Convert;
	using com.dalsemi.onewire;
	using com.dalsemi.onewire.adapter;

	/// <summary>
	/// <P> 1-Wire&#174 container for a Thermochron iButton, DS1921.
	/// This container encapsulates the functionality of the 1-Wire family type <B>21</B> (hex).
	/// </P>
	/// 
	/// <H3> Features </H3>
	/// <UL>
	///   <LI> Logs up to 2048 consecutive temperature measurements in nonvolatile, read-only memory
	///   <li> Real-Time clock with programmable alarm
	///   <LI> Programmable high and low temperature alarms
	///   <li> Alarm violation times and durations recorded in nonvolatile, read-only memory
	///   <li> Automatically 'wakes up' and logs temperature at user-programmable intervals
	///   <li> 4096 bits of general-purpose read/write nonvolatile memory
	///   <li> 256-bit scratchpad ensures integrity of data transfer
	///   <li> On-chip 16-bit CRC generator to verify read operations
	///   <li> Long-term histogram with 2&#176 C resolution
	/// </UL>
	/// 
	/// <H3> Memory </H3>
	/// 
	/// <P> The memory can be accessed through the objects that are returned
	/// from the <seealso cref="#getMemoryBanks() getMemoryBanks"/> method. </P>
	/// 
	/// The following is a list of the MemoryBank instances that are returned:
	/// 
	/// <UL>
	///   <LI> <B> Scratchpad with CRC </B>
	///      <UL>
	///         <LI> <I> Implements </I> <seealso cref="com.dalsemi.onewire.container.MemoryBank MemoryBank"/>,
	///                  <seealso cref="com.dalsemi.onewire.container.PagedMemoryBank PagedMemoryBank"/>
	///         <LI> <I> Size </I> 32 starting at physical address 0
	///         <LI> <I> Features</I> Read/Write not-general-purpose volatile
	///         <LI> <I> Pages</I> 1 page of length 32 bytes
	///         <LI> <I> Page Features </I> page-device-CRC
	///         <li> <i> Extra information for each page</i>  Target address, offset, length 3
	///      </UL>
	///   <LI> <B> Main Memory </B>
	///      <UL>
	///         <LI> <I> Implements </I> <seealso cref="com.dalsemi.onewire.container.MemoryBank MemoryBank"/>,
	///                  <seealso cref="com.dalsemi.onewire.container.PagedMemoryBank PagedMemoryBank"/>
	///         <LI> <I> Size </I> 512 starting at physical address 0
	///         <LI> <I> Features</I> Read/Write general-purpose non-volatile
	///         <LI> <I> Pages</I> 16 pages of length 32 bytes giving 29 bytes Packet data payload
	///         <LI> <I> Page Features </I> page-device-CRC
	///      </UL>
	///   <LI> <B> Register control </B>
	///      <UL>
	///         <LI> <I> Implements </I> <seealso cref="com.dalsemi.onewire.container.MemoryBank MemoryBank"/>,
	///                  <seealso cref="com.dalsemi.onewire.container.PagedMemoryBank PagedMemoryBank"/>
	///         <LI> <I> Size </I> 32 starting at physical address 512
	///         <LI> <I> Features</I> Read/Write not-general-purpose non-volatile
	///         <LI> <I> Pages</I> 1 pages of length 32 bytes
	///         <LI> <I> Page Features </I> page-device-CRC
	///      </UL>
	///   <LI> <B> Alarm time stamps </B>
	///      <UL>
	///         <LI> <I> Implements </I> <seealso cref="com.dalsemi.onewire.container.MemoryBank MemoryBank"/>,
	///                  <seealso cref="com.dalsemi.onewire.container.PagedMemoryBank PagedMemoryBank"/>
	///         <LI> <I> Size </I> 96 starting at physical address 544
	///         <LI> <I> Features</I> Read-only not-general-purpose non-volatile
	///         <LI> <I> Pages</I> 3 pages of length 32 bytes
	///         <LI> <I> Page Features </I> page-device-CRC
	///      </UL>
	///   <LI> <B> Temperature histogram </B>
	///      <UL>
	///         <LI> <I> Implements </I> <seealso cref="com.dalsemi.onewire.container.MemoryBank MemoryBank"/>,
	///                  <seealso cref="com.dalsemi.onewire.container.PagedMemoryBank PagedMemoryBank"/>
	///         <LI> <I> Size </I> 128 starting at physical address 2048
	///         <LI> <I> Features</I> Read-only not-general-purpose non-volatile
	///         <LI> <I> Pages</I> 4 pages of length 32 bytes
	///         <LI> <I> Page Features </I> page-device-CRC
	///      </UL>
	///   <LI> <B> Temperature log </B>
	///      <UL>
	///         <LI> <I> Implements </I> <seealso cref="com.dalsemi.onewire.container.MemoryBank MemoryBank"/>,
	///                  <seealso cref="com.dalsemi.onewire.container.PagedMemoryBank PagedMemoryBank"/>
	///         <LI> <I> Size </I> 2048 starting at physical address 4096
	///         <LI> <I> Features</I> Read-only not-general-purpose non-volatile
	///         <LI> <I> Pages</I> 64 pages of length 32 bytes
	///         <LI> <I> Page Features </I> page-device-CRC
	///      </UL>
	/// </UL>
	/// 
	/// <H3> Usage </H3>
	/// 
	/// <para>The code below starts a mission with the following characteristics:
	/// <ul>
	///     <li>Rollover flag enabled.  This means if more than 2048 samples are
	///         taken, the newer samples overwrite the oldest samples in the temperature
	///         log.</li>
	///     <li>High alarm of 28.0&#176 and a low alarm of 23.0&#176 C.  If the alarm is violated,
	///         the Temperature Alarm log will record when and for how long the violation occurred.</li>
	///     <li>The clock alarm enabled to Mondays at 12:30:45 pm.</li>
	///     <li>Sets the Thermocron's Real-Time Clock to the host system's clock.</li>
	///     <li>The mission will start in 2 minutes.</li>
	///     <li>A sample rate of 1 minute.</li>
	/// </ul>
	/// This code also ensures that the Thermocron's clock is set to run, and that the
	/// clock alarm is enabled.</para>
	/// <pre><code>
	///       // "ID" is a byte array of size 8 with an address of a part we
	///       // have already found with family code 12 hex
	///       // "access" is a DSPortAdapter
	///       OneWireContainer21 ds1921 = (OneWireContainer21) access.getDeviceContainer(ID);
	///       ds1921.setupContainer(access,ID);
	///       //  ds1921 previously setup as a OneWireContainer21
	///       ds1921.clearMemory();
	///       //  read the current state of the device
	///       byte[] state = ds1921.readDevice();
	///       //  enable rollover
	///       ds1921.setFlag(ds1921.CONTROL_REGISTER, ds1921.ROLLOVER_ENABLE_FLAG, true, state);
	///       //  set the high temperature alarm to 28 C
	///       ds1921.setTemperatureAlarm(ds1921.ALARM_HIGH, 28.0, state);
	///       //  set the low temperature alarm to 23 C
	///       ds1921.setTemperatureAlarm(ds1921.ALARM_LOW, 23.0, state);
	///       //  set the clock alarm to occur weekly, Mondays at 12:30:45 pm
	///       ds1921.setClockAlarm(12, 30, 45, 2, ds1921.ONCE_PER_WEEK, state);
	///       //  set the real time clock to the system's current clock
	///       ds1921.setClock(System.currentTimeMillis(), state);
	///       //  set the mission to start in 2 minutes
	///       ds1921.setMissionStartDelay(2,state);
	///       //  make sure the clock is set to run
	///       ds1921.setClockRunEnable(true, state);
	///       //  make sure the clock alarm is enabled
	///       ds1921.setClockAlarmEnable(true, state);
	///       //  write all that information out
	///       ds1921.writeDevice(state);
	///       //  now enable the mission with a sample rate of 1 minute
	///       ds1921.enableMission(1);
	/// </code></pre>
	/// 
	/// <para>The following code processes the temperature log:</para>
	/// <code><pre>
	///       byte[] state = ds1921.readDevice();
	///       byte[] log = ds1921.getTemperatureLog(state);
	///       Calendar time_stamp = ds1921.getMissionTimeStamp(state);
	///       long time = time_stamp.getTime().getTime() + ds1921.getFirstLogOffset(state);
	///       int sample_rate = ds1921.getSampleRate(state);
	/// 
	///       System.out.println("TEMPERATURE LOG");
	/// 
	///       for (int i=0;i &lt; log.length;i++)
	///       {
	///           System.out.println("- Temperature recorded at  : "+(new Date(time)));
	///           System.out.println("-                     was  : "+ds1921.decodeTemperature(log[i])+" C");
	///           time += sample_rate * 60 * 1000;
	///       }
	/// </pre></code>
	/// 
	/// <para>The following code processes the alarm histories:</para>
	/// <code><pre>
	///       byte[] high_history = ds1921.getAlarmHistory(ds1921.TEMPERATURE_HIGH_ALARM);
	///       byte[] low_history = ds1921.getAlarmHistory(ds1921.TEMPERATURE_LOW_ALARM);
	///       int sample_rate = ds1921.getSampleRate(state);
	///       int start_offset, violation_count;
	///       System.out.println("ALARM HISTORY");
	///       if (low_history.length==0)
	///       {
	///           System.out.println("- No violations against the low temperature alarm.");
	///           System.out.println("-");
	///       }
	///       for (int i=0;i &lt; low_history.length/4; i++)
	///       {
	///           start_offset = (low_history [i * 4] & 0x0ff)
	///                     | ((low_history [i * 4 + 1] &lt;&lt; 8) & 0x0ff00)
	///                     | ((low_history [i * 4 + 2] &lt;&lt; 16) & 0x0ff0000);
	///           violation_count = 0x0ff & low_history[i*4+3];
	///           System.out.println("- Low alarm started at     : "+(start_offset * sample_rate));
	///           System.out.println("-                          : Lasted "+(violation_count * sample_rate)+" minutes");
	///       }
	///       if (high_history.length==0)
	///       {
	///           System.out.println("- No violations against the high temperature alarm.");
	///           System.out.println("-");
	///       }
	///       for (int i=0;i &lt; high_history.length/4; i++)
	///       {
	///           start_offset = (high_history [i * 4] & 0x0ff)
	///                     | ((high_history [i * 4 + 1] &lt;&lt; 8) & 0x0ff00)
	///                     | ((high_history [i * 4 + 2] &lt;&lt; 16) & 0x0ff0000);
	///           violation_count = 0x0ff & high_history[i*4+3];
	///           System.out.println("- High alarm started at    : "+(start_offset * sample_rate));
	///           System.out.println("-                          : Lasted "+(violation_count * sample_rate)+" minutes");
	///       }
	/// </pre></code>
	/// 
	/// <para>The following code processes the temperature histogram:</para>
	/// <code><pre>
	///       double resolution = ds1921.getTemperatureResolution();
	///       double histBinWidth = ds1921.getHistogramBinWidth();
	///       double start = ds1921.getHistogramLowTemperature();
	///       System.out.println("TEMPERATURE HISTOGRAM");
	///       for (int i=0;i &lt; histogram.length;i++)
	///       {
	///          System.out.println("- Histogram entry          : "
	///                             + histogram [i] + " at temperature "
	///                             + start + " to "
	///                             + ( start + (histBinWidth - resolution)) + " C");
	///          start += histBinWidth;
	///       }
	/// </pre></code>
	/// 
	/// <para>Also see the usage examples in the <seealso cref="com.dalsemi.onewire.container.TemperatureContainer TemperatureContainer"/>
	/// and <seealso cref="com.dalsemi.onewire.container.ClockContainer ClockContainer"/>
	/// interfaces.</para>
	/// 
	/// For examples regarding memory operations,
	/// <uL>
	/// <li> See the usage example in
	/// <seealso cref="com.dalsemi.onewire.container.OneWireContainer OneWireContainer"/>
	/// to enumerate the MemoryBanks.
	/// <li> See the usage examples in
	/// <seealso cref="com.dalsemi.onewire.container.MemoryBank MemoryBank"/> and
	/// <seealso cref="com.dalsemi.onewire.container.PagedMemoryBank PagedMemoryBank"/>
	/// for bank specific operations.
	/// </uL>
	/// 
	/// <H3> DataSheet </H3>
	/// <DL>
	/// <DD><A HREF="http://pdfserv.maxim-ic.com/arpdf/DS1921L-F5X.pdf">http://pdfserv.maxim-ic.com/arpdf/DS1921L-F5X.pdf</A>
	/// </DL>
	/// 
	/// Also visit <a href="http://www.ibutton.com/ibuttons/thermochron.html">
	/// http://www.ibutton.com/ibuttons/thermochron.html</a> for links to more
	/// sources on the DS1921 Thermocron.
	/// </summary>
	/// <seealso cref= com.dalsemi.onewire.container.OneWireSensor </seealso>
	/// <seealso cref= com.dalsemi.onewire.container.SwitchContainer </seealso>
	/// <seealso cref= com.dalsemi.onewire.container.TemperatureContainer
	/// 
	/// @version    0.00, 28 Aug 2000
	/// @author     COlmstea, KLA
	///  </seealso>
	public class OneWireContainer21 : OneWireContainer, TemperatureContainer, ClockContainer
	{
	   private bool doSpeedEnable = true;

	   /* privates!
	    * Memory commands.
	    */
	   private static readonly byte WRITE_SCRATCHPAD_COMMAND = 0x0F;
	   private static readonly byte READ_SCRATCHPAD_COMMAND = 0xAA;
	   private static readonly byte COPY_SCRATCHPAD_COMMAND = 0x55;
	   private static readonly byte READ_MEMORY_CRC_COMMAND = 0xA5;
	   private static readonly byte CLEAR_MEMORY_COMMAND = 0x3C;
	   private static readonly byte CONVERT_TEMPERATURE_COMMAND = 0x44;

	   // Scratchpad access memory bank
	   private MemoryBankScratchCRC scratch;

	   // Register control memory bank
	   private MemoryBankNVCRC register;

	   // Alarms memory bank
	   private MemoryBankNVCRC alarm;

	   // Histogram memory bank
	   private MemoryBankNVCRC histogram;

	   // Log memory bank
	   private MemoryBankNVCRC log;

	   // Buffer to hold the temperature log in
	   private byte[] read_log_buffer = new byte [64 * 32]; //64 pages X 32 bytes per page

	   // should we update the Real time clock?
	   private bool updatertc = false;

	   // Maxim/Dallas Semiconductor Part number
	   private string partNumber = "DS1921";

	   // Temperature range low temperaturein degrees Celsius
	   // calculated through 12-bit field of 1-Wire Net Address
	   private double temperatureRangeLow = -40.0;
	   private double temperatureRangeHigh = 85.0;

	   // Temperature range width in degrees Celsius
	   // calculated through 12-bit field of 1-Wire Net Address
	   //private double temperatureRangeWidth = 125.0;

	   // Temperature resolution in degrees Celsius
	   // calculated through 12-bit field of 1-Wire Net Address
	   private double temperatureResolution = 0.5;

	   // The temperature range at which the device will operate.
	   private double temperatureOperatingRangeLow = -40.0;
	   private double temperatureOperatingRangeHigh = 85.0;

	   // Is this 1-Wire device a DS1921HZ?
	   private bool isDS1921HZ = false;


	   /////////////////////////////////////////////
	   //
	   //PUBLIC's
	   //
	   /////////////////////////////////////////////

	   /// <summary>
	   /// Address of the status register. Used with the <code>getFlag</code>
	   /// and <code>setFlag</code> methods to set and
	   /// check flags indicating the Thermochron's status.
	   /// </summary>
	   /// <seealso cref= #getFlag(int,byte,byte[]) </seealso>
	   /// <seealso cref= #getFlag(int,byte) </seealso>
	   /// <seealso cref= #setFlag(int,byte,bool) </seealso>
	   /// <seealso cref= #setFlag(int,byte,bool,byte[]) </seealso>
	   public const int STATUS_REGISTER = 0x214;

	   /// <summary>
	   /// Address of the control register. Used with the <code>getFlag</code>
	   /// and <code>setFlag</code> methods to set and
	   /// check flags indicating the Thermochron's status.
	   /// </summary>
	   /// <seealso cref= #getFlag(int,byte,byte[]) </seealso>
	   /// <seealso cref= #getFlag(int,byte) </seealso>
	   /// <seealso cref= #setFlag(int,byte,bool) </seealso>
	   /// <seealso cref= #setFlag(int,byte,bool,byte[]) </seealso>
	   public const int CONTROL_REGISTER = 0x20E;

	   /// <summary>
	   /// Alarm frequency setting for the <code>setClockAlarm()</code> method.
	   /// If the DS1921 Thermocron alarm is enabled and is not alarming,
	   /// it will alarm on the next Real-Time Clock second.
	   /// </summary>
	   /// <seealso cref= #setClockAlarm(int,int,int,int,int,byte[]) </seealso>
	   public const byte ONCE_PER_SECOND = (byte) 0x1F;

	   /// <summary>
	   /// Alarm frequency setting for the <code>setClockAlarm()</code> method.
	   /// If the DS1921 Thermocron alarm is enabled and is not alarming,
	   /// it will alarm the next time the Real-Time Clock's 'second' value is
	   /// equal to the Alarm Clock's 'second' value.
	   /// </summary>
	   /// <seealso cref= #setClockAlarm(int,int,int,int,int,byte[]) </seealso>
	   public const byte ONCE_PER_MINUTE = (byte) 0x17;

	   /// <summary>
	   /// Alarm frequency setting for the <code>setClockAlarm()</code> method.
	   /// If the DS1921 Thermocron alarm is enabled and is not alarming,
	   /// it will alarm the next time the Real-Time Clock's 'second' and 'minute' values are
	   /// equal to the Alarm Clock's 'second' and 'minute' values.
	   /// </summary>
	   /// <seealso cref= #setClockAlarm(int,int,int,int,int,byte[]) </seealso>
	   public const byte ONCE_PER_HOUR = (byte) 0x13;

	   /// <summary>
	   /// Alarm frequency setting for the <code>setClockAlarm()</code> method.
	   /// If the DS1921 Thermocron alarm is enabled and is not alarming,
	   /// it will alarm the next time the Real-Time Clock's 'second', 'minute', and 'hour' values are
	   /// equal to the Alarm Clock's 'second', 'minute', and 'hour' values.
	   /// </summary>
	   /// <seealso cref= #setClockAlarm(int,int,int,int,int,byte[]) </seealso>
	   public const byte ONCE_PER_DAY = (byte) 0x11;

	   /// <summary>
	   /// Alarm frequency setting for the <code>setClockAlarm()</code> method.
	   /// If the DS1921 Thermocron alarm is enabled and is not alarming,
	   /// it will alarm the next time the Real-Time Clock's 'second', 'minute', 'hour', and 'day of week' values are
	   /// equal to the Alarm Clock's 'second', 'minute', 'hour', and 'day of week' values
	   /// </summary>
	   /// <seealso cref= #setClockAlarm(int,int,int,int,int,byte[]) </seealso>
	   public const byte ONCE_PER_WEEK = (byte) 0x10;

	   /// <summary>
	   /// Low temperature alarm value for the methods <code>getAlarmStatus()</code>,
	   /// <code>getAlarmHistory()</code>, and <code>setTemperatureAlarm()</code>.
	   /// </summary>
	   /// <seealso cref= #getAlarmStatus(byte,byte[]) </seealso>
	   /// <seealso cref= #getAlarmHistory(byte) </seealso>
	   /// <seealso cref= #setTemperatureAlarm(int,double,byte[]) </seealso>
	   public const byte TEMPERATURE_LOW_ALARM = 4;

	   /// <summary>
	   /// High temperature alarm value for the methods <code>getAlarmStatus()</code>,
	   /// <code>getAlarmHistory()</code>, and <code>setTemperatureAlarm()</code>.
	   /// </summary>
	   /// <seealso cref= #getAlarmStatus(byte,byte[]) </seealso>
	   /// <seealso cref= #getAlarmHistory(byte) </seealso>
	   /// <seealso cref= #setTemperatureAlarm(int,double,byte[]) </seealso>
	   public const byte TEMPERATURE_HIGH_ALARM = 2;

	   /// <summary>
	   /// Clock alarm value for the methods <code>getAlarmStatus()</code>
	   /// and <code>isClockAlarming()</code>.
	   /// </summary>
	   /// <seealso cref= #getAlarmStatus(byte,byte[]) </seealso>
	   /// <seealso cref= #isClockAlarming(byte[]) </seealso>
	   public const byte TIMER_ALARM = 1;

	   /// <summary>
	   /// CONTROL REGISTER FLAG: When enabled, the device will respond to conditional
	   /// search command if a timer alarm has occurred.
	   /// </summary>
	   /// <seealso cref= #getFlag(int,byte,byte[]) </seealso>
	   /// <seealso cref= #getFlag(int,byte) </seealso>
	   /// <seealso cref= #setFlag(int,byte,bool) </seealso>
	   /// <seealso cref= #setFlag(int,byte,bool,byte[]) </seealso>
	   public const byte TIMER_ALARM_SEARCH_FLAG = 1;

	   /// <summary>
	   /// CONTROL REGISTER FLAG: When enabled, the device will respond to conditional
	   /// search command if the temperature has reached the high temperature threshold.
	   /// </summary>
	   /// <seealso cref= #getFlag(int,byte,byte[]) </seealso>
	   /// <seealso cref= #getFlag(int,byte) </seealso>
	   /// <seealso cref= #setFlag(int,byte,bool) </seealso>
	   /// <seealso cref= #setFlag(int,byte,bool,byte[]) </seealso>
	   public const byte TEMP_HIGH_SEARCH_FLAG = 2;

	   /// <summary>
	   /// CONTROL REGISTER FLAG: When enabled, the device will respond to conditional
	   /// search command if the temperature has reached the low temperature threshold.
	   /// </summary>
	   /// <seealso cref= #getFlag(int,byte,byte[]) </seealso>
	   /// <seealso cref= #getFlag(int,byte) </seealso>
	   /// <seealso cref= #setFlag(int,byte,bool) </seealso>
	   /// <seealso cref= #setFlag(int,byte,bool,byte[]) </seealso>
	   public const byte TEMP_LOW_SEARCH_FLAG = 4;

	   /// <summary>
	   /// CONTROL REGISTER FLAG: When enabled, the device will begin overwriting the earlier
	   /// temperature measurements when the temperature log memory becomes full.
	   /// </summary>
	   /// <seealso cref= #getFlag(int,byte,byte[]) </seealso>
	   /// <seealso cref= #getFlag(int,byte) </seealso>
	   /// <seealso cref= #setFlag(int,byte,bool) </seealso>
	   /// <seealso cref= #setFlag(int,byte,bool,byte[]) </seealso>
	   public const byte ROLLOVER_ENABLE_FLAG = 8;

	   /// <summary>
	   /// CONTROL REGISTER FLAG: When DISABLED, the mission will start as soon as the
	   /// sample rate is written.
	   /// </summary>
	   /// <seealso cref= #getFlag(int,byte,byte[]) </seealso>
	   /// <seealso cref= #getFlag(int,byte) </seealso>
	   /// <seealso cref= #setFlag(int,byte,bool) </seealso>
	   /// <seealso cref= #setFlag(int,byte,bool,byte[]) </seealso>
	   public const byte MISSION_ENABLE_FLAG = 16;

	   /// <summary>
	   /// CONTROL REGISTER FLAG: Must be enabled to allow a clear memory
	   /// function. Must be set immediately before the command is issued.
	   /// </summary>
	   /// <seealso cref= #clearMemory() </seealso>
	   /// <seealso cref= #getFlag(int,byte,byte[]) </seealso>
	   /// <seealso cref= #getFlag(int,byte) </seealso>
	   /// <seealso cref= #setFlag(int,byte,bool) </seealso>
	   /// <seealso cref= #setFlag(int,byte,bool,byte[]) </seealso>
	   public const byte MEMORY_CLEAR_ENABLE_FLAG = 64;

	   /// <summary>
	   /// CONTROL REGISTER FLAG: When DISABLED, the real time clock will start
	   /// working. Must be disabled for normal operation.
	   /// </summary>
	   /// <seealso cref= #getFlag(int,byte,byte[]) </seealso>
	   /// <seealso cref= #getFlag(int,byte) </seealso>
	   /// <seealso cref= #setFlag(int,byte,bool) </seealso>
	   /// <seealso cref= #setFlag(int,byte,bool,byte[]) </seealso>
	   public static readonly byte OSCILLATOR_ENABLE_FLAG =  128;

	   /// <summary>
	   /// STATUS REGISTER FLAG: Will read back true when a clock alarm has occurred.
	   /// </summary>
	   /// <seealso cref= #getFlag(int,byte,byte[]) </seealso>
	   /// <seealso cref= #getFlag(int,byte) </seealso>
	   public const byte TIMER_ALARM_FLAG = 1;

	   /// <summary>
	   /// STATUS REGISTER FLAG:  Will read back true when the temperature during a mission
	   /// reaches or exceeds the temperature high threshold.
	   /// </summary>
	   /// <seealso cref= #getFlag(int,byte,byte[]) </seealso>
	   /// <seealso cref= #getFlag(int,byte) </seealso>
	   public const byte TEMPERATURE_HIGH_FLAG = 2;

	   /// <summary>
	   /// STATUS REGISTER FLAG: Will read back true when a temperature equal to or below
	   /// the low temperature threshold was detected on a mission.
	   /// </summary>
	   /// <seealso cref= #getFlag(int,byte,byte[]) </seealso>
	   /// <seealso cref= #getFlag(int,byte) </seealso>
	   public const byte TEMPERATURE_LOW_FLAG = 4;

	   /// <summary>
	   /// STATUS REGISTER FLAG: Will read back true when a mission temperature conversion
	   /// is in progress
	   /// </summary>
	   /// <seealso cref= #getFlag(int,byte,byte[]) </seealso>
	   /// <seealso cref= #getFlag(int,byte) </seealso>
	   public const byte SAMPLE_IN_PROGRESS_FLAG = 16;

	   /// <summary>
	   /// STATUS REGISTER FLAG: Will read back true when a mission is in progress.
	   /// </summary>
	   /// <seealso cref= #getFlag(int,byte,byte[]) </seealso>
	   /// <seealso cref= #getFlag(int,byte) </seealso>
	   public const byte MISSION_IN_PROGRESS_FLAG = 32;

	   /// <summary>
	   /// STATUS REGISTER FLAG: Will read back true if the memory has been cleared.
	   /// </summary>
	   /// <seealso cref= #getFlag(int,byte,byte[]) </seealso>
	   /// <seealso cref= #getFlag(int,byte) </seealso>
	   public const byte MEMORY_CLEARED_FLAG = 64;

	   /// <summary>
	   /// STATUS REGISTER FLAG: Will read back true if a temperature conversion
	   /// of any kind is in progress.
	   /// </summary>
	   /// <seealso cref= #getFlag(int,byte,byte[]) </seealso>
	   /// <seealso cref= #getFlag(int,byte) </seealso>
	   public const byte TEMP_CORE_BUSY_FLAG =  128;

	   /// <summary>
	   /// Creates a new <code>OneWireContainer</code> for communication with a DS1921 Thermocron iButton.
	   /// Note that the method <code>setupContainer(DSPortAdapter,byte[])</code>
	   /// must be called to set the correct <code>DSPortAdapter</code> device address.
	   /// </summary>
	   /// <seealso cref= com.dalsemi.onewire.container.OneWireContainer#setupContainer(com.dalsemi.onewire.adapter.DSPortAdapter,byte[]) setupContainer(DSPortAdapter,byte[]) </seealso>
	   /// <seealso cref= #OneWireContainer21(com.dalsemi.onewire.adapter.DSPortAdapter,byte[]) OneWireContainer21(DSPortAdapter,byte[]) </seealso>
	   /// <seealso cref= #OneWireContainer21(com.dalsemi.onewire.adapter.DSPortAdapter,long)   OneWireContainer21(DSPortAdapter,long) </seealso>
	   /// <seealso cref= #OneWireContainer21(com.dalsemi.onewire.adapter.DSPortAdapter,java.lang.String) OneWireContainer21(DSPortAdapter,String) </seealso>
	   public OneWireContainer21() : base()
	   {

		  // initialize the memory banks
		  initMem();
	   }

	   /// <summary>
	   /// Creates a new <code>OneWireContainer</code> for communication with a DS1921 Thermocron iButton.
	   /// </summary>
	   /// <param name="sourceAdapter">     adapter object required to communicate with
	   /// this iButton </param>
	   /// <param name="newAddress">        address of this DS1921
	   /// </param>
	   /// <seealso cref= #OneWireContainer21() </seealso>
	   /// <seealso cref= #OneWireContainer21(com.dalsemi.onewire.adapter.DSPortAdapter,long)   OneWireContainer21(DSPortAdapter,long) </seealso>
	   /// <seealso cref= #OneWireContainer21(com.dalsemi.onewire.adapter.DSPortAdapter,java.lang.String) OneWireContainer21(DSPortAdapter,String) </seealso>
	   public OneWireContainer21(DSPortAdapter sourceAdapter, byte[] newAddress) : base(sourceAdapter, newAddress)
	   {

		  // initialize the memory banks
		  initMem();
	   }

	   /// <summary>
	   /// Creates a new <code>OneWireContainer</code> for communication with a DS1921 Thermocron iButton.
	   /// </summary>
	   /// <param name="sourceAdapter">     adapter object required to communicate with
	   /// this iButton </param>
	   /// <param name="newAddress">        address of this DS1921
	   /// </param>
	   /// <seealso cref= #OneWireContainer21() </seealso>
	   /// <seealso cref= #OneWireContainer21(com.dalsemi.onewire.adapter.DSPortAdapter,byte[]) OneWireContainer21(DSPortAdapter,byte[]) </seealso>
	   /// <seealso cref= #OneWireContainer21(com.dalsemi.onewire.adapter.DSPortAdapter,java.lang.String) OneWireContainer21(DSPortAdapter,String) </seealso>
	   public OneWireContainer21(DSPortAdapter sourceAdapter, long newAddress) : base(sourceAdapter, newAddress)
	   {

		  // initialize the memory banks
		  initMem();
	   }

	   /// <summary>
	   /// Creates a new <code>OneWireContainer</code> for communication with a DS1921 Thermocron iButton.
	   /// </summary>
	   /// <param name="sourceAdapter">     adapter object required to communicate with
	   /// this iButton </param>
	   /// <param name="newAddress">        address of this DS1921
	   /// </param>
	   /// <seealso cref= #OneWireContainer21() </seealso>
	   /// <seealso cref= #OneWireContainer21(com.dalsemi.onewire.adapter.DSPortAdapter,long) OneWireContainer21(DSPortAdapter,long) </seealso>
	   /// <seealso cref= #OneWireContainer21(com.dalsemi.onewire.adapter.DSPortAdapter,java.lang.String) OneWireContainer21(DSPortAdapter,String) </seealso>
	   public OneWireContainer21(DSPortAdapter sourceAdapter, string newAddress) : base(sourceAdapter, newAddress)
	   {

		  // initialize the memory banks
		  initMem();
	   }

	   /// <summary>
	   /// Provides this container with the adapter object used to access this device and
	   /// the address of the iButton or 1-Wire device.
	   /// </summary>
	   /// <param name="sourceAdapter">     adapter object required to communicate with
	   ///                           this iButton </param>
	   /// <param name="newAddress">        address of this 1-Wire device </param>
	   /// <seealso cref= com.dalsemi.onewire.utils.Address </seealso>
	   public override void setupContainer(DSPortAdapter sourceAdapter, byte[] newAddress)
	   {
		  base.setupContainer(sourceAdapter, newAddress);
		  setThermochronVariables();
	   }

	   /// <summary>
	   /// Provides this container with the adapter object used to access this device and
	   /// the address of the iButton or 1-Wire device.
	   /// </summary>
	   /// <param name="sourceAdapter">     adapter object required to communicate with
	   ///                           this iButton </param>
	   /// <param name="newAddress">        address of this 1-Wire device </param>
	   /// <seealso cref= com.dalsemi.onewire.utils.Address </seealso>
	   public override void setupContainer(DSPortAdapter sourceAdapter, long newAddress)
	   {
		  base.setupContainer(sourceAdapter, newAddress);
		  setThermochronVariables();
	   }

	   /// <summary>
	   /// Provides this container with the adapter object used to access this device and
	   /// the address of the iButton or 1-Wire device.
	   /// </summary>
	   /// <param name="sourceAdapter">     adapter object required to communicate with
	   ///                           this iButton </param>
	   /// <param name="newAddress">        address of this 1-Wire device </param>
	   /// <seealso cref= com.dalsemi.onewire.utils.Address </seealso>
	   public override void setupContainer(DSPortAdapter sourceAdapter, string newAddress)
	   {
		  base.setupContainer(sourceAdapter, newAddress);
		  setThermochronVariables();
	   }


	   /// <summary>
	   /// Gets an enumeration of memory bank instances that implement one or more
	   /// of the following interfaces:
	   /// <seealso cref="com.dalsemi.onewire.container.MemoryBank MemoryBank"/>,
	   /// <seealso cref="com.dalsemi.onewire.container.PagedMemoryBank PagedMemoryBank"/>,
	   /// and <seealso cref="com.dalsemi.onewire.container.OTPMemoryBank OTPMemoryBank"/>. </summary>
	   /// <returns> <CODE>Enumeration</CODE> of memory banks </returns>
	   public override IEnumerator MemoryBanks
	   {
		   get
		   {
			  List<object> bank_vector = new List<object>(6);
    
			  // scratchpad
			  bank_vector.Add(scratch);
    
			  // NVRAM
			  bank_vector.Add(new MemoryBankNVCRC(this, scratch));
    
			  // Register page
			  bank_vector.Add(register);
    
			  // Alarm time stamps and duration
			  bank_vector.Add(alarm);
    
			  // Histogram
			  bank_vector.Add(histogram);
    
			  // Log
			  bank_vector.Add(log);
    
			  return bank_vector.GetEnumerator();
		   }
	   }

	   //--------
	   //-------- Private
	   //--------

	   /// <summary>
	   /// Construct the memory banks used for I/O.
	   /// </summary>
	   private void initMem()
	   {

		  // scratchpad
		  scratch = new MemoryBankScratchCRC(this);

		  // Register
		  register = new MemoryBankNVCRC(this, scratch);
		  register.numberPages = 1;
		  register.size = 32;
		  register.bankDescription = "Register control";
		  register.startPhysicalAddress = 0x200;
		  register.generalPurposeMemory = false;

		  // Alarm registers
		  alarm = new MemoryBankNVCRC(this, scratch);
		  alarm.numberPages = 3;
		  alarm.size = 96;
		  alarm.bankDescription = "Alarm time stamps";
		  alarm.startPhysicalAddress = 544;
		  alarm.generalPurposeMemory = false;
		  alarm.readOnly = true;
		  alarm.readWrite = false;

		  // Histogram
		  histogram = new MemoryBankNVCRC(this, scratch);
		  histogram.numberPages = 4;
		  histogram.size = 128;
		  histogram.bankDescription = "Temperature Histogram";
		  histogram.startPhysicalAddress = 2048;
		  histogram.generalPurposeMemory = false;
		  histogram.readOnly = true;
		  histogram.readWrite = false;

		  // Log
		  log = new MemoryBankNVCRC(this, scratch);
		  log.numberPages = 64;
		  log.size = 2048;
		  log.bankDescription = "Temperature log";
		  log.startPhysicalAddress = 4096;
		  log.generalPurposeMemory = false;
		  log.readOnly = true;
		  log.readWrite = false;
	   }

	   /// <summary>
	   /// Sets the following, calculated from the
	   /// 12-bit code of the 1-Wire Net Address:
	   /// (All temperatures set to degrees Celsius)
	   /// 1)  The part numbers:
	   ///     DS1921L-F50 = physical range -40 to +85,
	   ///                   operating range -40 to +85.
	   ///     DS1921L-F51 = physical range -40 to +85,
	   ///                   operating range -10 to +85.
	   ///     DS1921L-F52 = physical range -40 to +85,
	   ///                   operating range -20 to +85.
	   ///     DS1921L-F53 = physical range -40 to +85,
	   ///                   operating range -30 to +85.
	   /// 
	   ///     DS1921H     = physical range 15 to 46,
	   ///                   operating range -40 to +85
	   ///     DS1921Z     = physical range -5 to 26,
	   ///                   operating range -40 to +85
	   /// 2)  Temperature Range low temperature.
	   /// 3)  Temperature Range width in degrees Celsius.
	   /// 4)  Temperature Resolution.
	   /// 5)  If a DS1921H or DS1921Z is detected.
	   /// 
	   /// </summary>
	   private void setThermochronVariables()
	   {
		  // Get Temperature Range code, which is the first 12 (MSB) bits of the
		  // unique serial number (after the CRC).
		  byte[] address = Address; // retrieve 1-Wire net address to look at range code.
		  int rangeCode = (((address[6] & 0x0FF) << 4) | ((address[5] & 0x0FF)>>4));

		  switch (rangeCode)
		  {
			 case 0x34C:
				partNumber = "DS1921L-F51";
				temperatureRangeLow = -40;
				temperatureRangeHigh = 85;
				temperatureResolution = 0.5;
				temperatureOperatingRangeLow = -10;
				temperatureOperatingRangeHigh = 85;
				isDS1921HZ = false;
				break;
			 case 0x254:
				partNumber = "DS1921L-F52";
				temperatureRangeLow = -40;
				temperatureRangeHigh = 85;
				temperatureResolution = 0.5;
				temperatureOperatingRangeLow = -20;
				temperatureOperatingRangeHigh = 85;
				isDS1921HZ = false;
				break;
			 case 0x15C:
				partNumber = "DS1921L-F53";
				temperatureRangeLow = -40;
				temperatureRangeHigh = 85;
				temperatureResolution = 0.5;
				temperatureOperatingRangeLow = -30;
				temperatureOperatingRangeHigh = 85;
				isDS1921HZ = false;
				break;
			 case 0x4F2:
				partNumber = "DS1921H-F5";
				temperatureRangeLow = 15;
				temperatureRangeHigh = 46;
				temperatureResolution = 0.125;
				temperatureOperatingRangeLow = -40;
				temperatureOperatingRangeHigh = 85;
				isDS1921HZ = true;
				break;
			 case 0x3B2:
				partNumber = "DS1921Z-F5";
				temperatureRangeLow = -5;
				temperatureRangeHigh = 26;
				temperatureResolution = 0.125;
				temperatureOperatingRangeLow = -40;
				temperatureOperatingRangeHigh = 85;
				isDS1921HZ = true;
				break;
			 default:
				long lower36bits = (((long)address[5] & 0x0F) << 32) | (((long)address[4] & 0x0FF) << 24) | (((long)address[3] & 0x0FF) << 16) | (((long)address[2] & 0x0FF) << 8) | ((long)address[1] & 0x0FF);
				if (lower36bits >= 0x100000)
				{
				   partNumber = "DS1921G-F5";
				}
				else
				{
				   partNumber = "DS1921L-PROTO";
				}

				temperatureRangeLow = -40;
				temperatureRangeHigh = 85;
				temperatureResolution = 0.5;
				temperatureOperatingRangeLow = -40;
				temperatureOperatingRangeHigh = 85;
				isDS1921HZ = false;
				break;

		  }
		  /*
		  // Get Temperature Range code, which is the first 12 (MSB) bits of the
		  // unique serial number (after the CRC).
		  byte[] netAddress = getAddress(); // retrieve 1-Wire net address to look at range code.
		  int rangeCode = (netAddress[6] & 0xFF); // And with 0xFF to get rid of sign extension
		  rangeCode = rangeCode << 8;   // left shift 8 bits to put most significant byte in correct place
		  rangeCode = rangeCode + (netAddress[5] & 0xFF);  // add the least significant byte to make integer.
		  rangeCode = rangeCode >> 4;   // this is a 12-bit number, so get rid of extra 4 bits.
	
		  // Detect what kind of part we have, a DS1921L-F5X or a DS1921H/Z
		  int detectionInt = rangeCode & 0x03;  // get the last 2 bits to see what they are
		  if (detectionInt > 0) isDS1921HZ = true; // if the last 2 bits > 0 then the part is a DS1921H or Z
	
		  // Get temperature ranges as a result of the rangeCode and the type of device.
		  if (isDS1921HZ)
		  {
		     // get the most significant 8 bits of the 12-bit rangeCode
		     temperatureRangeLow = rangeCode >> 4;
		     temperatureRangeLow = temperatureRangeLow - 64; // 1 degree increment with 0x000 = -64 degrees.
	
		     // Resolution Code -- the last 2 bits of the 12-bit rangeCode number
		     //
		     // 0 = 0.5 degrees Celsius
		     // 1 = 0.25
		     // 2 = 0.125
		     // 3 = 0.0625
		     switch(rangeCode & 0x03) // gets the last 2 bits of the 12-bit rangeCode.
		     {
		        case 0:
		           temperatureResolution = 0.5;
		           break;
		        case 1:
		           temperatureResolution = 0.25;
		           break;
		        case 2:
		           temperatureResolution = 0.125;
		           break;
		        case 3:
		           temperatureResolution = 0.0625;
		           break;
		        default:
		           temperatureResolution = 0.5;
		     }
	
		     // Range Modifier Code (range width)
		     //
		     // 0 = full range, 256 * resolution
		     // 1 = reduced range, 2/3 of full range
		     // 2 = reduced range, 1/2 of full range
		     // 3 = reduced range, 1/3 of full range
	
		     switch((rangeCode >> 2) & 0x03)
		     {
		        case 0:
		           temperatureRangeWidth = (256 * temperatureResolution) - 1;
		           break;
		        case 1:
		           temperatureRangeWidth = (256 * temperatureResolution * 2 / 3) - 1;
		           break;
		        case 2:
		           temperatureRangeWidth = (256 * temperatureResolution / 2) - 1;
		           break;
		        case 3:
		           temperatureRangeWidth = (256 * temperatureResolution / 3) - 1;
		           break;
		        default:
		           temperatureRangeWidth = (256 * temperatureResolution) - 1;
		     }
		  }
		  else
		  {
		     // get the most significant 5 bits of the 12-bit rangeCode number.
		     temperatureRangeLow = rangeCode >> 7;
		     temperatureRangeLow = (temperatureRangeLow * 5) - 40;  // 5 degree increment
		     temperatureResolution = 0.5;  // for non DS1921H/Z parts, the resolution is the same.
		     // if part has a range code, get the next 5 bits of the 12-bit number.
		     if (rangeCode > 0) temperatureRangeWidth = ((rangeCode >> 2) & (0x1F)) * 5; // 5 degree increment
		  }
		  // set the part number based (currently) on low temperature.
		  switch((int) temperatureRangeLow) // switches on the low temperature.
		  {
		     case 15:
		        partNumber = "DS1921H-F5";
		        break;
		     case -5:
		        partNumber = "DS1921Z-F5";
		        break;
		     case -10:
		        partNumber = "DS1921L-F51";
		        break;
		     case -20:
		        partNumber = "DS1921L-F52";
		        break;
		     case -30:
		        partNumber = "DS1921L-F53";
		        break;
		     case -40:
		        partNumber = "DS1921L-F50";
		        break;
		     default:
		        partNumber = "DS1921";
		  }*/
	   }

	   /// <summary>
	   /// Grab the date from one of the time registers.
	   /// returns int[] = {year, month, date}
	   /// </summary>
	   private int[] getDate(int timeReg, byte[] state)
	   {
		  byte upper, lower;
		  int[] result = new int [3];

		  timeReg = timeReg & 31;

		  /* extract the day of the month */
		  lower = state [timeReg++];
		  upper = (byte)(((int)((uint)lower >> 4)) & 0x0f);
		  lower = (byte)(lower & 0x0f);
		  result [2] = 10 * upper + lower;

		  /* extract the month */
		  lower = state [timeReg++];
		  upper = (byte)(((int)((uint)lower >> 4)) & 0x0f);
		  lower = (byte)(lower & 0x0f);

		  // the upper bit contains the century, so subdivide upper
		  byte century = (byte)(((int)((uint)upper >> 3)) & 0x01);

		  upper = (byte)(upper & 0x01);
		  result [1] = lower + upper * 10;

		  /* grab the year */
		  result [0] = 1900 + century * 100;
		  lower = state [timeReg++];
		  upper = (byte)(((int)((uint)lower >> 4)) & 0x0f);
		  lower = (byte)(lower & 0x0f);
		  result [0] += upper * 10 + lower;

		  return result;
	   }

	   /// <summary>
	   /// Gets the time of day fields in 24-hour time from button
	   /// returns int[] = {seconds, minutes, hours}
	   /// </summary>
	   private int[] getTime(int timeReg, byte[] state)
	   {
		  byte upper, lower;
		  int[] result = new int [3];

		  timeReg = timeReg & 31;

		  // NOTE: The MSbit is ANDed out (with the 0x07) because alarm clock
		  //       registers have an extra bit to indicate alarm frequency

		  /* First grab the seconds. Upper half holds the 10's of seconds       */
		  lower = state [timeReg++];
		  upper = (byte)(((int)((uint)lower >> 4)) & 0x07);
		  lower = (byte)(lower & 0x0f);
		  result [0] = (int) lower + (int) upper * 10;

		  /* now grab minutes. The upper half holds the 10s of minutes          */
		  lower = state [timeReg++];
		  upper = (byte)(((int)((uint)lower >> 4)) & 0x07);
		  lower = (byte)(lower & 0x0f);
		  result [1] = (int) lower + (int) upper * 10;

		  /* now grab the hours. The lower half is single hours again, but the
		     upper half of the byte is determined by the 2nd bit - specifying
		     12/24 hour time. */
		  lower = state [timeReg++];
		  upper = (byte)(((int)((uint)lower >> 4)) & 0x07);
		  lower = (byte)(lower & 0x0f);

		  int hours;

		  // if the 2nd bit is 1, convert 12 hour time to 24 hour time.
		  if (((int)((uint)upper >> 2)) != 0)
		  {

			 // extract the AM/PM byte (PM is indicated by a 1)
			 byte PM = (byte)(((int)((uint)(upper << 6) >> 7)) & 0x01);

			 // isolate the 10s place
			 upper = (byte)(upper & 0x01);
			 hours = upper * 10 + PM * 12;
		  }
		  else
		  {
			 hours = upper * 10; // already in 24 hour format
		  }

		  hours += lower;
		  result [2] = hours;

		  return result;
	   }

	   /// <summary>
	   /// Set the time in the DS1921 time register format.
	   /// </summary>
	   private void setTime(int timeReg, int hours, int minutes, int seconds, bool AMPM, byte[] state)
	   {
		  byte upper, lower;

		  /* format in bytes and write seconds */
		  upper = (byte)(seconds / 10);
		  upper = (byte)((upper << 4) & 0xf0);
		  lower = (byte)(seconds % 10);
		  lower = (byte)(lower & 0x0f);
		  state [timeReg & 31] = (byte)(upper | lower);

		  timeReg++;

		  /* format in bytes and write minutes */
		  upper = (byte)(minutes / 10);
		  upper = (byte)((upper << 4) & 0xf0);
		  lower = (byte)(minutes % 10);
		  lower = (byte)(lower & 0x0f);
		  state [timeReg & 31] = (byte)(upper | lower);

		  timeReg++;

		  /* format in bytes and write hours/(12/24) bit */
		  if (AMPM)
		  {
			 upper = (byte) 0x04;

			 if (hours > 11)
			 {
				upper = (byte)(upper | 0x02);
			 }

			 // this next function simply checks for a decade hour
			 if (((hours % 12) == 0) || ((hours % 12) > 9))
			 {
				upper = (byte)(upper | 0x01);
			 }

			 if (hours > 12)
			 {
				hours = hours - 12;
			 }

			 if (hours == 0)
			 {
				lower = 0x02;
			 }
			 else
			 {
				lower = (byte)((hours % 10) & 0x0f);
			 }
		  }
		  else
		  {
			 upper = (byte)(hours / 10);
			 lower = (byte)(hours % 10);
		  }

		  upper = (byte)((upper << 4) & 0xf0);
		  lower = (byte)(lower & 0x0f);
		  state [timeReg & 31] = (byte)(upper | lower);

		  timeReg++;
	   }

	   /// <summary>
	   /// Set the current date in the DS1921's real time clock.
	   /// 
	   /// year - The year to set to, i.e. 2001.
	   /// month - The month to set to, i.e. 1 for January, 12 for December.
	   /// day - The day of month to set to, i.e. 1 to 31 in January, 1 to 30 in April.
	   /// </summary>
	   private void setDate(int year, int month, int day, byte[] state)
	   {
		  byte upper, lower;

		  /* write the day byte (the upper holds 10s of days, lower holds single days) */
		  upper = (byte)(day / 10);
		  upper = (byte)((upper << 4) & 0xf0);
		  lower = (byte)(day % 10);
		  lower = (byte)(lower & 0x0f);
		  state [0x04] = (byte)(upper | lower);

		  /* write the month bit in the same manner, with the MSBit indicating
		     the century (1 for 2000, 0 for 1900) */
		  upper = (byte)(month / 10);
		  upper = (byte)((upper << 4) & 0xf0);
		  lower = (byte)(month % 10);
		  lower = (byte)(lower & 0x0f);

		  if (year > 1999)
		  {
			 upper = (byte)(upper | 128);

			 //go ahead and fix up the year too while i'm at it
			 year = year - 2000;
		  }
		  else
		  {
			 year = year - 1900;
		  }

		  state [0x05] = (byte)(upper | lower);

		  // now write the year
		  upper = (byte)(year / 10);
		  upper = (byte)((upper << 4) & 0xf0);
		  lower = (byte)(year % 10);
		  lower = (byte)(lower & 0x0f);
		  state [0x06] = (byte)(upper | lower);
	   }

	   //////////////////////////////////////////////////////////////
	   //
	   //   Public methods
	   //
	   //////////////////////////////////////////////////////////////


	   /// <summary>
	   /// Returns the maximum speed this iButton device can
	   /// communicate at.
	   /// </summary>
	   /// <returns> maximum speed </returns>
	   /// <seealso cref= DSPortAdapter#setSpeed </seealso>
	   public override int MaxSpeed
	   {
		   get
		   {
			  return DSPortAdapter.SPEED_OVERDRIVE;
		   }
	   }

	   /// <summary>
	   /// Gets the Dallas Semiconductor part number of the iButton
	   /// or 1-Wire Device as a <code>java.lang.String</code>.
	   /// For example "DS1992".
	   /// </summary>
	   /// <returns> iButton or 1-Wire device name </returns>
	   public override string Name
	   {
		   get
		   {
			  return partNumber;
		   }
	   }

	   /// <summary>
	   /// Retrieves the alternate Dallas Semiconductor part numbers or names.
	   /// A 'family' of MicroLAN devices may have more than one part number
	   /// depending on packaging.  There can also be nicknames such as
	   /// "Crypto iButton".
	   /// </summary>
	   /// <returns>  the alternate names for this iButton or 1-Wire device </returns>
	   public override string AlternateNames
	   {
		   get
		   {
			  return "Thermochron";
		   }
	   }

	   /// <summary>
	   /// Gets a short description of the function of this iButton
	   /// or 1-Wire Device type.
	   /// </summary>
	   /// <returns> device description </returns>
	   public override string Description
	   {
		   get
		   {
			  // put the DS1921's characteristics together in a string format.
			  string characteristics = "";
			  if (!string.ReferenceEquals(partNumber, "DS1921"))
			  {
				 // get the physical range as a string
				 string strPhysicalRange = Convert.ToString(PhysicalRangeLowTemperature,1) + " to " + Convert.ToString(PhysicalRangeHighTemperature,1) + " degrees Celsius.";
				 // get the operating range as a string
				 string strOperatingRange = Convert.ToString(OperatingRangeLowTemperature,1) + " to " + Convert.ToString(OperatingRangeHighTemperature,1) + " degrees Celsius.";
				 characteristics = " The operating range for this device is:  " + strOperatingRange + " The physical range for this device is:  " + strPhysicalRange + " The resolution is " + Convert.ToString(TemperatureResolution,3) + " degrees Celsius, and the histogram bin width is " + Convert.ToString(HistogramBinWidth,3) + " degrees Celsius.";
			  }
			  string returnString = "Rugged, self-sufficient 1-Wire device that, once setup for " + "a mission, will measure the temperature and record the result in " + "a protected memory section. It stores up to 2048 temperature " + "measurements and will take measurements at a user-specified " + "rate. The thermochron also records the number of times the temperature " + "falls on a given degree range (temperature bin), and stores the " + "data in histogram format." + characteristics;
    
			  return returnString;
		   }
	   }

	   /// <summary>
	   /// Directs the container to avoid the calls to doSpeed() in methods that communicate
	   /// with the Thermocron. To ensure that all parts can talk to the 1-Wire bus
	   /// at their desired speed, each method contains a call
	   /// to <code>doSpeed()</code>.  However, this is an expensive operation.
	   /// If a user manages the bus speed in an
	   /// application,  call this method with <code>doSpeedCheck</code>
	   /// as <code>false</code>.  The default behavior is
	   /// to call <code>doSpeed()</code>.
	   /// </summary>
	   /// <param name="doSpeedCheck"> <code>true</code> for <code>doSpeed()</code> to be called before every
	   /// 1-Wire bus access, <code>false</code> to skip this expensive call
	   /// </param>
	   /// <seealso cref= OneWireContainer#doSpeed() </seealso>
	   public virtual bool SpeedCheck
	   {
		   set
		   {
			   lock (this)
			   {
				  doSpeedEnable = value;
			   }
		   }
	   }

	   /// <summary>
	   /// This method returns the low temperature of
	   /// the thermochron's physical temperature range.
	   /// The physical range is the range of temperatures
	   /// that the thermochron can record.
	   /// 
	   /// The following is a list of physical ranges in
	   /// degrees Celsius:
	   /// 
	   ///     DS1921L-F5X = physical range -40 to +85
	   /// 
	   ///     DS1921H     = physical range 15 to 46
	   /// 
	   ///     DS1921Z     = physical range -5 to 26
	   /// </summary>
	   /// <returns> the physical range low temperature in degrees Celsius </returns>
	   public virtual double PhysicalRangeLowTemperature
	   {
		   get
		   {
			  return temperatureRangeLow;
		   }
	   }

	   /// <summary>
	   /// This method returns the high temperature of
	   /// the thermochron's physical temperature range.
	   /// The physical range is the range of temperatures
	   /// that the thermochron can record.
	   /// 
	   /// The following is a list of physical ranges in
	   /// degrees Celsius:
	   /// 
	   ///     DS1921L-F5X = physical range -40 to +85
	   /// 
	   ///     DS1921H     = physical range 15 to 46
	   /// 
	   ///     DS1921Z     = physical range -5 to 26
	   /// </summary>
	   /// <returns> the physical range low temperature in degrees Celsius </returns>
	   public virtual double PhysicalRangeHighTemperature
	   {
		   get
		   {
			  return temperatureRangeHigh;
		   }
	   }

	   /// <summary>
	   /// This method returns the low temperature of
	   /// the thermochron's operating temperature range.
	   /// The operating range is the range of temperatures
	   /// for which the thermochron can function properly.
	   /// 
	   /// The following is a list of operating ranges in
	   /// degrees Celsius:
	   /// 
	   ///     DS1921L-F50 = operating range -40 to +85.
	   ///     DS1921L-F51 = operating range -10 to +85.
	   ///     DS1921L-F52 = operating range -20 to +85.
	   ///     DS1921L-F53 = operating range -30 to +85.
	   /// 
	   ///     DS1921H     = operating range -40 to +85
	   ///     DS1921Z     = operating range -40 to +85
	   /// </summary>
	   /// <returns> the operating range low temperature in degrees Celsius </returns>
	   public virtual double OperatingRangeLowTemperature
	   {
		   get
		   {
			  return temperatureOperatingRangeLow;
		   }
	   }

	   /// <summary>
	   /// This method returns the high temperature of
	   /// the thermochron's operating temperature range.
	   /// The operating range is the range of temperatures
	   /// for which the thermochron can function properly.
	   /// 
	   /// The following is a list of operating ranges in
	   /// degrees Celsius:
	   /// 
	   ///     DS1921L-F50 = operating range -40 to +85.
	   ///     DS1921L-F51 = operating range -10 to +85.
	   ///     DS1921L-F52 = operating range -20 to +85.
	   ///     DS1921L-F53 = operating range -30 to +85.
	   /// 
	   ///     DS1921H     = operating range -40 to +85
	   ///     DS1921Z     = operating range -40 to +85
	   /// </summary>
	   /// <returns> the operating range high temperature in degrees Celsius </returns>
	   public virtual double OperatingRangeHighTemperature
	   {
		   get
		   {
			  return temperatureOperatingRangeHigh;
		   }
	   }

	   /// <summary>
	   /// Retrieves the resolution with which the thermochron takes
	   /// temperatures in degrees Celsius.
	   /// </summary>
	   /// <returns> the temperature resolution of this thermochron. </returns>
	   public virtual double TemperatureResolution
	   {
		   get
		   {
			  return temperatureResolution;
		   }
	   }

	   /// <summary>
	   /// Retrieves the lowest temperature of the first histogram bin
	   /// in degrees Celsius.
	   /// </summary>
	   /// <returns> the lowest histogram bin temperature. </returns>
	   public virtual double HistogramLowTemperature
	   {
		   get
		   {
			  double lowTemp = PhysicalRangeLowTemperature; // low temp of thermochrons other than H or Z
			  if (isDS1921HZ)
			  {
				  lowTemp = lowTemp - (TemperatureResolution * 4);
			  }
			  return lowTemp;
		   }
	   }

	   /// <summary>
	   /// This method returns the width of a histogram bin in degrees
	   /// Celsius.
	   /// </summary>
	   /// <returns> the width of a histogram bin for this thermochron. </returns>
	   public virtual double HistogramBinWidth
	   {
		   get
		   {
			  return (TemperatureResolution * 4); // 4 temperature readings per bin
		   }
	   }

	   /// <summary>
	   /// Converts a temperature from the DS1921 <code>byte</code> encoded
	   /// format to degrees Celsius.  The raw temperature readings are unsigned
	   /// <code>byte</code> values, representing a 2.0 degree accuracy.
	   /// </summary>
	   /// <param name="tempByte"> raw DS1921 temperature reading
	   /// </param>
	   /// <returns> temperature in degrees Celsius
	   /// </returns>
	   /// <seealso cref= #encodeTemperature(double) </seealso>
	   public virtual double decodeTemperature(byte tempByte)
	   {
		  // the formula for DS1921H/Z:
		  // C = Tbyte * Tres + (Tlow - (4 * Tres))
		  // where C is decimal degrees Celsius.
		  // and Tbyte is the byte to be decoded.
		  // and Tlow is the low temperature of temperature range.
		  // and Tres is the resolution of the DS1921.

		  double decodedTemperature = 0.0;
		  if (isDS1921HZ)
		  {
			 decodedTemperature = ((tempByte & 0x00ff) * temperatureResolution);
			 decodedTemperature = decodedTemperature + (temperatureRangeLow - (4 * temperatureResolution));
		  }
		  else
		  {
			 decodedTemperature = ((tempByte & 0x00ff) / 2.0) - 40.0;
		  }
		  return decodedTemperature;
	   }

	   /// <summary>
	   /// Converts a temperature in degrees Celsius to
	   /// a <code>byte</code> formatted for the DS1921.
	   /// </summary>
	   /// <param name="temperature"> the temperature (Celsius) to convert
	   /// </param>
	   /// <returns> the temperature in raw DS1921 format
	   /// </returns>
	   /// <seealso cref= #decodeTemperature(byte) </seealso>
	   public virtual byte encodeTemperature(double temperature)
	   {
		  // the formula for DS1921H/Z:
		  // Tbyte = ((C - Tlow) / Tres) + 4;
		  // where Tbyte is the byte to be encoded.
		  // and C is decimal degrees Celsius
		  // and Tlow is the low temperature of temperature range
		  // and Tres is the resolution of the DS1921

		  byte encodedTemperature = 0x00;
		  if (isDS1921HZ)
		  {
			 double result = ((temperature - temperatureRangeLow) / temperatureResolution) + 4;
			 encodedTemperature = (byte)((int) result & 0x000000ff);
		  }
		  else
		  {
			 encodedTemperature = (byte)(((int)(2 * temperature) + 80) & 0x000000ff);
		  }
		  return encodedTemperature;
	   }

	   /// <summary>
	   /// Writes a byte of data into the DS1921's memory. Note that writing to
	   /// the register page while a mission is in progress ends that mission.
	   /// Also note that the preferred way to write a page is through the
	   /// <code>MemoryBank</code> objects returned from the <code>getMemoryBanks()</code>
	   /// method.
	   /// </summary>
	   /// <param name="memAddr"> the address for writing (in the range of 0x200-0x21F) </param>
	   /// <param name="source"> the data <code>byte</code> to write
	   /// </param>
	   /// <exception cref="OneWireIOException"> on a 1-Wire communication error such as
	   ///         reading an incorrect CRC from a 1-Wire device.  This could be
	   ///         caused by a physical interruption in the 1-Wire Network due to
	   ///         shorts or a newly arriving 1-Wire device issuing a 'presence pulse'. </exception>
	   /// <exception cref="OneWireException"> on a communication or setup error with the 1-Wire
	   ///         adapter
	   /// </exception>
	   /// <seealso cref= #readByte(int) </seealso>
	   /// <seealso cref= #getMemoryBanks() </seealso>
	   public virtual void writeByte(int memAddr, byte source)
	   {

		  // User should only need to write to the 32 byte register page
		  byte[] buffer = new byte [5];

		  // break the address into its bytes
		  byte msbAddress = (byte)(((int)((uint)memAddr >> 8)) & 0x0ff);
		  byte lsbAddress = (byte)(memAddr & 0x0ff);

		  /* check for valid parameters */
		  if ((msbAddress > 0x1F) || (msbAddress < 0))
		  {
			 throw new System.ArgumentException("OneWireContainer21-Address for write out of range.");
		  }

		  /* perform the write and verification */
		  if (doSpeedEnable)
		  {
			 doSpeed();
		  }

		  if (adapter.select(address))
		  {

			 /* write to the scratchpad first */
			 buffer [0] = WRITE_SCRATCHPAD_COMMAND;
			 buffer [1] = lsbAddress;
			 buffer [2] = msbAddress;
			 buffer [3] = source;

			 adapter.dataBlock(buffer, 0, 4);

			 /* read it back for the verification bytes required to copy it to mem */
			 adapter.select(address);

			 buffer [0] = READ_SCRATCHPAD_COMMAND;

			 for (int i = 1; i < 5; i++)
			 {
				buffer [i] = 0x0ff;
			 }

			 adapter.dataBlock(buffer, 0, 5);

			 // check to see if the data was written correctly
			 if (buffer [4] != source)
			 {
				throw new OneWireIOException("OneWireContainer21-Error writing data byte.");
			 }

			 /* now perform the copy from the scratchpad to memory */
			 adapter.select(address);

			 buffer [0] = COPY_SCRATCHPAD_COMMAND;

			 // keep buffer[1]-buffer[3] because they contain the verification bytes
			 buffer [4] = 0xff;

			 adapter.dataBlock(buffer, 0, 5);

			 /* now check to see that the part sent a 01010101 indicating a success */
			 if ((buffer [4] != 0xAA) && (buffer [4] != (byte) 0x55))
			 {
				throw new OneWireIOException("OneWireContainer21-Error writing data byte.");
			 }
		  }
		  else
		  {
			 throw new OneWireException("OneWireContainer21-Device not present.");
		  }
	   }

	   /// <summary>
	   /// Reads a single byte from the DS1921.  Note that the preferred manner
	   /// of reading from the DS1921 Thermocron is through the <code>readDevice()</code>
	   /// method or through the <code>MemoryBank</code> objects returned in the
	   /// <code>getMemoryBanks()</code> method.
	   /// </summary>
	   /// <param name="memAddr"> the address to read from  (in the range of 0x200-0x21F)
	   /// </param>
	   /// <returns> the data byte read
	   /// </returns>
	   /// <exception cref="OneWireIOException"> on a 1-Wire communication error such as
	   ///         reading an incorrect CRC from a 1-Wire device.  This could be
	   ///         caused by a physical interruption in the 1-Wire Network due to
	   ///         shorts or a newly arriving 1-Wire device issuing a 'presence pulse'. </exception>
	   /// <exception cref="OneWireException"> on a communication or setup error with the 1-Wire
	   ///         adapter
	   /// </exception>
	   /// <seealso cref= #writeByte(int,byte) </seealso>
	   /// <seealso cref= #readDevice() </seealso>
	   /// <seealso cref= #getMemoryBanks() </seealso>
	   public virtual byte readByte(int memAddr)
	   {
		  byte[] buffer = new byte [4];

		  // break the address up into bytes
		  byte msbAddress = (byte)((memAddr >> 8) & 0x000000ff);
		  byte lsbAddress = (byte)(memAddr & 0x000000ff);

		  /* check the validity of the address */
		  if ((msbAddress > 0x1F) || (msbAddress < 0))
		  {
			 throw new System.ArgumentException("OneWireContainer21-Address for read out of range.");
		  }

		  /* read a user specified amount of memory and verify its validity */
		  if (doSpeedEnable)
		  {
			 doSpeed();
		  }

		  if (adapter.select(address))
		  {
			 buffer [0] = READ_MEMORY_CRC_COMMAND;
			 buffer [1] = lsbAddress;
			 buffer [2] = msbAddress;
			 buffer [3] = 0x0ff;

			 adapter.dataBlock(buffer, 0, 4);

			 return buffer [3];
		  }
		  else
		  {
			 throw new OneWireException("OneWireContainer21-Device not present.");
		  }
	   }

	   /// <summary>
	   /// <para>Gets the status of the specified flag from the specified register.
	   /// This method actually communicates with the Thermocron.  To improve
	   /// performance if you intend to make multiple calls to this method,
	   /// first call <code>readDevice()</code> and use the
	   /// <code>getFlag(int, byte, byte[])</code> method instead.</para>
	   /// 
	   /// <para>The DS1921 Thermocron has two sets of flags.  One set belongs
	   /// to the control register.  When reading from the control register,
	   /// valid values for <code>bitMask</code> are:</para>
	   /// <ul>
	   ///     <li><code> TIMER_ALARM_SEARCH_FLAG  </code></li>
	   ///     <li><code> TEMP_HIGH_SEARCH_FLAG    </code></li>
	   ///     <li><code> TEMP_LOW_SEARCH_FLAG     </code></li>
	   ///     <li><code> ROLLOVER_ENABLE_FLAG     </code></li>
	   ///     <li><code> MISSION_ENABLE_FLAG      </code></li>
	   ///     <li><code> MEMORY_CLEAR_ENABLE_FLAG </code></li>
	   ///     <li><code> OSCILLATOR_ENABLE_FLAG   </code></li>
	   /// </ul>
	   /// <para>When reading from the status register, valid values
	   /// for <code>bitMask</code> are:</para>
	   /// <ul>
	   ///     <li><code> TIMER_ALARM_FLAG         </code></li>
	   ///     <li><code> TEMPERATURE_HIGH_FLAG    </code></li>
	   ///     <li><code> TEMPERATURE_LOW_FLAG     </code></li>
	   ///     <li><code> SAMPLE_IN_PROGRESS_FLAG  </code></li>
	   ///     <li><code> MISSION_IN_PROGRESS_FLAG </code></li>
	   ///     <li><code> MEMORY_CLEARED_FLAG      </code></li>
	   ///     <li><code> TEMP_CORE_BUSY_FLAG      </code></li>
	   /// </ul>
	   /// </summary>
	   /// <param name="register"> address of register containing the flag (valid values
	   /// are <code>CONTROL_REGISTER</code> and <code>STATUS_REGISTER</code>) </param>
	   /// <param name="bitMask"> the flag to read (see above for available options)
	   /// </param>
	   /// <returns> the status of the flag, where <code>true</code>
	   /// signifies a "1" and <code>false</code> signifies a "0"
	   /// </returns>
	   /// <exception cref="OneWireIOException"> on a 1-Wire communication error such as
	   ///         reading an incorrect CRC from a 1-Wire device.  This could be
	   ///         caused by a physical interruption in the 1-Wire Network due to
	   ///         shorts or a newly arriving 1-Wire device issuing a 'presence pulse'. </exception>
	   /// <exception cref="OneWireException"> on a communication or setup error with the 1-Wire
	   ///         adapter
	   /// </exception>
	   /// <seealso cref= #getFlag(int,byte,byte[]) </seealso>
	   /// <seealso cref= #readDevice() </seealso>
	   /// <seealso cref= #setFlag(int,byte,bool) </seealso>
	   /// <seealso cref= #TIMER_ALARM_SEARCH_FLAG </seealso>
	   /// <seealso cref= #TEMP_HIGH_SEARCH_FLAG </seealso>
	   /// <seealso cref= #TEMP_LOW_SEARCH_FLAG </seealso>
	   /// <seealso cref= #ROLLOVER_ENABLE_FLAG </seealso>
	   /// <seealso cref= #MISSION_ENABLE_FLAG </seealso>
	   /// <seealso cref= #MEMORY_CLEAR_ENABLE_FLAG </seealso>
	   /// <seealso cref= #OSCILLATOR_ENABLE_FLAG </seealso>
	   /// <seealso cref= #TIMER_ALARM_FLAG </seealso>
	   /// <seealso cref= #TEMPERATURE_HIGH_FLAG </seealso>
	   /// <seealso cref= #TEMPERATURE_LOW_FLAG </seealso>
	   /// <seealso cref= #SAMPLE_IN_PROGRESS_FLAG </seealso>
	   /// <seealso cref= #MISSION_IN_PROGRESS_FLAG </seealso>
	   /// <seealso cref= #MEMORY_CLEARED_FLAG </seealso>
	   /// <seealso cref= #TEMP_CORE_BUSY_FLAG
	   /// 
	   ///  </seealso>
	   public virtual bool getFlag(int register, byte bitMask)
	   {
		  return ((readByte(register) & bitMask) != 0);
	   }

	   /// <summary>
	   /// <para>Gets the status of the specified flag from the specified register.
	   /// This method is the preferred manner of reading the control and
	   /// status flags.</para>
	   /// 
	   /// <para>For more information on valid values for the <code>bitMask</code>
	   /// parameter, see the <seealso cref="#getFlag(int,byte) getFlag(int,byte)"/> method.</para>
	   /// </summary>
	   /// <param name="register"> address of register containing the flag (valid values
	   /// are <code>CONTROL_REGISTER</code> and <code>STATUS_REGISTER</code>) </param>
	   /// <param name="bitMask"> the flag to read (see <seealso cref="#getFlag(int,byte) getFlag(int,byte)"/>
	   /// for available options) </param>
	   /// <param name="state"> current state of the device returned from <code>readDevice()</code>
	   /// </param>
	   /// <returns> the status of the flag, where <code>true</code>
	   /// signifies a "1" and <code>false</code> signifies a "0"
	   /// </returns>
	   /// <seealso cref= #getFlag(int,byte) </seealso>
	   /// <seealso cref= #readDevice() </seealso>
	   /// <seealso cref= #setFlag(int,byte,bool,byte[]) </seealso>
	   public virtual bool getFlag(int register, byte bitMask, byte[] state)
	   {
		  return ((state [register & 31] & bitMask) != 0);
	   }

	   /// <summary>
	   /// <para>Sets the status of the specified flag in the specified register.
	   /// If a mission is in progress a <code>OneWireIOException</code> will be thrown
	   /// (one cannot write to the registers while a mission is commencing).  This method
	   /// actually communicates with the DS1921 Thermocron.  To improve
	   /// performance if you intend to make multiple calls to this method,
	   /// first call <code>readDevice()</code> and use the
	   /// <code>setFlag(int,byte,bool,byte[])</code> method instead.</para>
	   /// 
	   /// <para>For more information on valid values for the <code>bitMask</code>
	   /// parameter, see the <seealso cref="#getFlag(int,byte) getFlag(int,byte)"/> method.</para>
	   /// </summary>
	   /// <param name="register"> address of register containing the flag (valid values
	   /// are <code>CONTROL_REGISTER</code> and <code>STATUS_REGISTER</code>) </param>
	   /// <param name="bitMask"> the flag to read (see <seealso cref="#getFlag(int,byte) getFlag(int,byte)"/>
	   /// for available options) </param>
	   /// <param name="flagValue"> new value for the flag (<code>true</code> is logic "1")
	   /// </param>
	   /// <exception cref="OneWireIOException"> on a 1-Wire communication error such as
	   ///         reading an incorrect CRC from a 1-Wire device.  This could be
	   ///         caused by a physical interruption in the 1-Wire Network due to
	   ///         shorts or a newly arriving 1-Wire device issuing a 'presence pulse'.
	   ///         In the case of the DS1921 Thermocron, this could also be due to a
	   ///         currently running mission. </exception>
	   /// <exception cref="OneWireException"> on a communication or setup error with the 1-Wire
	   ///         adapter
	   /// </exception>
	   /// <seealso cref= #getFlag(int,byte) </seealso>
	   /// <seealso cref= #getFlag(int,byte,byte[]) </seealso>
	   /// <seealso cref= #setFlag(int,byte,bool,byte[]) </seealso>
	   /// <seealso cref= #readDevice() </seealso>
	   public virtual void setFlag(int register, byte bitMask, bool flagValue)
	   {

		  // check for Mission in Progress flag
		  if (getFlag(STATUS_REGISTER, MISSION_IN_PROGRESS_FLAG))
		  {
			 throw new OneWireIOException("OneWireContainer21-Cannot write to register while mission is in progress.");
		  }

		  // read the current flag settings
		  byte flags = readByte(register);

		  if (flagValue)
		  {
			 flags = (byte)(flags | bitMask);
		  }
		  else
		  {
			 flags = (byte)(flags & ~(bitMask));
		  }

		  // write the regs back
		  writeByte(register, flags);
	   }

	   /// <summary>
	   /// <para>Sets the status of the specified flag in the specified register.
	   /// If a mission is in progress a <code>OneWireIOException</code> will be thrown
	   /// (one cannot write to the registers while a mission is commencing).  This method
	   /// is the preferred manner of setting the DS1921 status and control flags.
	   /// The method <code>writeDevice()</code> must be called to finalize
	   /// changes to the device.  Note that multiple 'set' methods can
	   /// be called before one call to <code>writeDevice()</code>.</para>
	   /// 
	   /// <para>For more information on valid values for the <code>bitMask</code>
	   /// parameter, see the <seealso cref="#getFlag(int,byte) getFlag(int,byte)"/> method.</para>
	   /// </summary>
	   /// <param name="register"> address of register containing the flag (valid values
	   /// are <code>CONTROL_REGISTER</code> and <code>STATUS_REGISTER</code>) </param>
	   /// <param name="bitMask"> the flag to read (see <seealso cref="#getFlag(int,byte) getFlag(int,byte)"/>
	   /// for available options) </param>
	   /// <param name="flagValue"> new value for the flag (<code>true</code> is logic "1") </param>
	   /// <param name="state"> current state of the device returned from <code>readDevice()</code>
	   /// </param>
	   /// <seealso cref= #getFlag(int,byte) </seealso>
	   /// <seealso cref= #getFlag(int,byte,byte[]) </seealso>
	   /// <seealso cref= #setFlag(int,byte,bool) </seealso>
	   /// <seealso cref= #readDevice() </seealso>
	   /// <seealso cref= #writeDevice(byte[]) </seealso>
	   public virtual void setFlag(int register, byte bitMask, bool flagValue, byte[] state)
	   {
		  register = register & 31;

		  byte flags = state [register];

		  if (flagValue)
		  {
			 flags = (byte)(flags | bitMask);
		  }
		  else
		  {
			 flags = (byte)(flags & ~(bitMask));
		  }

		  // write the regs back
		  state [register] = flags;
	   }

	   /// <summary>
	   /// <para>Begins this DS1921's mission. If a mission is
	   /// already in progress, this will throw a <code>OneWireIOException</code>.
	   /// The mission will wait the number of minutes specified by the
	   /// mission start delay (use <code>setMissionStartDelay()</code>)
	   /// before beginning.</para>
	   /// 
	   /// <para>Note that this method actually communicates with the DS1921
	   /// Thermocron.  No call to <code>writeDevice()</code> is required to
	   /// finalize mission enabling.  However, some flags (such as the mission
	   /// start delay) may need to be set with a call to <code>writeDevice()</code>
	   /// before the mission is enabled.  See the usage section
	   /// above for an example of starting a mission.</para>
	   /// </summary>
	   /// <param name="sampleRate"> the number of minutes to wait in between temperature samples
	   /// (valid values are 1 to 255)
	   /// </param>
	   /// <exception cref="OneWireIOException"> on a 1-Wire communication error such as
	   ///         reading an incorrect CRC from a 1-Wire device.  This could be
	   ///         caused by a physical interruption in the 1-Wire Network due to
	   ///         shorts or a newly arriving 1-Wire device issuing a 'presence pulse'.
	   ///         In the case of the DS1921 Thermocron, this could also be due to a
	   ///         currently running mission. </exception>
	   /// <exception cref="OneWireException"> on a communication or setup error with the 1-Wire
	   ///         adapter
	   /// </exception>
	   /// <seealso cref= #disableMission() </seealso>
	   /// <seealso cref= #setMissionStartDelay(int,byte[]) </seealso>
	   /// <seealso cref= #writeDevice(byte[]) </seealso>
	   public virtual void enableMission(int sampleRate)
	   {
		  /* check for valid parameters */
		  if ((sampleRate > 255) || (sampleRate < 0))
		  {
			 throw new System.ArgumentException("OneWireContainer21-Sample rate must be 255 minutes or less");
		  }

		  if (getFlag(STATUS_REGISTER, MISSION_IN_PROGRESS_FLAG))
		  {
			 throw new OneWireIOException("OneWireContainer30-Unable to start mission (Mission already in Progress)");
		  }

		  // read the current register status
		  byte controlReg = readByte(CONTROL_REGISTER);

		  // Set the enable mission byte to 0
		  controlReg = (byte)(controlReg & 0xEF);

		  writeByte(CONTROL_REGISTER, controlReg);

		  // set the sample rate and let her rip
		  writeByte(0x20D, (byte)(sampleRate & 0x000000ff));
	   }

	   /// <summary>
	   /// Ends this DS1921's running mission.  Note that this method
	   /// actually communicates with the DS1921 Thermocron.  No additional
	   /// call to <code>writeDevice(byte[])</code> is required.
	   /// </summary>
	   /// <exception cref="OneWireIOException"> on a 1-Wire communication error such as
	   ///         reading an incorrect CRC from a 1-Wire device.  This could be
	   ///         caused by a physical interruption in the 1-Wire Network due to
	   ///         shorts or a newly arriving 1-Wire device issuing a 'presence pulse'. </exception>
	   /// <exception cref="OneWireException"> on a communication or setup error with the 1-Wire
	   ///         adapter
	   /// </exception>
	   /// <seealso cref= #enableMission(int) </seealso>
	   public virtual void disableMission()
	   {

		  // first read the current register
		  byte statusReg = readByte(STATUS_REGISTER);

		  // Set the MIP bit to 0, regardless of whether a mission is commencing
		  statusReg = (byte)(statusReg & 0xDF); // set the MIP bit to 0;

		  writeByte(STATUS_REGISTER, statusReg);
	   }

	   /// <summary>
	   /// <para>Sets the time to wait before starting the mission.
	   /// The DS1921 will sleep <code>missionStartDelay</code>
	   /// minutes after the mission is enabled with <code>enableMission(int)</code>,
	   /// then begin taking samples.  Only the least significant 16 bits of
	   /// <code>missionStartDelay</code> are relevant.</para>
	   /// 
	   /// <para>The method <code>writeDevice()</code> must be called to finalize
	   /// changes to the device.  Note that multiple 'set' methods can
	   /// be called before one call to <code>writeDevice()</code>.</para>
	   /// </summary>
	   /// <param name="missionStartDelay"> the time in minutes to delay the first sample </param>
	   /// <param name="state"> current state of the device returned from <code>readDevice()</code>
	   /// </param>
	   /// <seealso cref= #readDevice() </seealso>
	   /// <seealso cref= #writeDevice(byte[]) </seealso>
	   /// <seealso cref= #enableMission(int) </seealso>
	   public virtual void setMissionStartDelay(int missionStartDelay, byte[] state)
	   {
		  state [0x12] = (byte)(missionStartDelay);
		  state [0x13] = (byte)(missionStartDelay >> 8);
	   }

	   /// <summary>
	   /// <para>Clears the memory of any previous mission.  The memory
	   /// must be cleared before setting up a new mission. If a
	   /// mission is in progress a <code>OneWireIOException</code> is thrown.</para>
	   /// 
	   /// <para>The Clear Memory command clears the Thermocron's memory
	   /// at address 220h and higher.  It also clears the sample rate, mission
	   /// start delay, mission time stamp, and mission samples counter.</para>
	   /// 
	   /// <para>Note that this method actually communicates with the DS1921 Thermocron.
	   /// No call to <code>writeDevice(byte[])</code> is necessary to finalize this
	   /// activity.</para>
	   /// </summary>
	   /// <exception cref="OneWireIOException"> on a 1-Wire communication error such as
	   ///         reading an incorrect CRC from a 1-Wire device.  This could be
	   ///         caused by a physical interruption in the 1-Wire Network due to
	   ///         shorts or a newly arriving 1-Wire device issuing a 'presence pulse'.
	   ///         In the case of the DS1921 Thermocron, this could also be due to a
	   ///         currently running mission. </exception>
	   /// <exception cref="OneWireException"> on a communication or setup error with the 1-Wire
	   ///         adapter
	   /// </exception>
	   /// <seealso cref= #enableMission(int) </seealso>
	   /// <seealso cref= #writeDevice(byte[]) </seealso>
	   public virtual void clearMemory()
	   {
		  // added 8/29/2001 by SH - delay necessary so that clock is
		  // running before mission is enabled.
		  // check to see if the Oscillator is enabled.
		  byte[] state = readDevice();
		  if (isClockRunning(state))
		  {
			 // if the osciallator is not enabled, start it
			 setClockRunEnable(true, state);
			 writeDevice(state);
			 // and give it the required time
			 try
			 {
				Thread.Sleep(751);
			 }
			 catch (InterruptedException)
			 {
				 ;
			 }
		  }

		  // first set the MCLRE bit to 1 in the control register
		  setFlag(CONTROL_REGISTER, MEMORY_CLEAR_ENABLE_FLAG, true);

		  // now send the memory clear command and wait 5 milliseconds
		  if (doSpeedEnable)
		  {
			 doSpeed();
		  }

		  adapter.reset();

		  if (adapter.select(address))
		  {
			 adapter.putByte(CLEAR_MEMORY_COMMAND);

			 try
			 {
				Thread.Sleep(5);
			 }
			 catch (System.Exception)
			 {
				//drain it
			 }
		  }
		  else
		  {
			 throw new OneWireException("OneWireContainer21-Device not found.");
		  }
	   }

	   /// <summary>
	   /// <para>Gets the clock alarm time settings.  The alarm settings used by the
	   /// Thermocron are Hour, Minute, Second, and Day of Week.  Note that not
	   /// all values in the returned <code>java.util.Calendar</code> object
	   /// are valid.  Only four values in the <code>Calendar</code> should
	   /// be used.  The field names for these values are:<pre><code>
	   ///      Calendar.DAY_OF_MONTH
	   ///      Calendar.HOUR_OF_DAY
	   ///      Calendar.MINUTE
	   ///      Calendar.SECOND</code></pre>
	   /// </para>
	   /// <para>The hour is reported in 24-hour format.  Use the method <code>getClockAlarm(byte[])</code>
	   /// to find out the next time an alarm event will occur.</para>
	   /// </summary>
	   /// <param name="state"> current state of the device returned from <code>readDevice()</code>
	   /// </param>
	   /// <returns> the alarm clock time and day of the week
	   /// </returns>
	   /// <seealso cref= #setClockAlarm(int,int,int,int,int,byte[]) </seealso>
	   /// <seealso cref= #readDevice() </seealso>
	   /// <seealso cref= #getClockAlarm(byte[]) </seealso>
	   public virtual DateTime getAlarmTime(byte[] state)
	   {

		  // first get the time
		  int[] time = getTime(0x207, state);
          DateTime result = new DateTime(0, 0, 0, time[2], time[1], time[0]);

		  // now put the day of the week in there
		  //byte dayOfWeek = (byte)(state [0x0A] & 0x07);

		  //result.set(DateTime.DAY_OF_MONTH, dayOfWeek);

		  return result;
	   }

	   /// <summary>
	   /// Set the DS1921's alarm clock.  Some of the parameters
	   /// might be unimportant depending on the alarm frequency setting.
	   /// For instance, if the alarm frequency setting is <code>ONCE_PER_MINUTE</code>,
	   /// then the <code>hour</code> argument is irrelevant.</p>
	   /// 
	   /// <para>Valid values for <code>alarmFrequency</code> are:<pre><code>
	   ///    ONCE_PER_SECOND
	   ///    ONCE_PER_MINUTE
	   ///    ONCE_PER_HOUR
	   ///    ONCE_PER_DAY
	   ///    ONCE_PER_WEEK</code></pre>
	   /// </para>
	   /// 
	   /// <para>The method <code>writeDevice()</code> must be called to finalize
	   /// changes to the device.  Note that multiple 'set' methods can
	   /// be called before one call to <code>writeDevice()</code>.</para>
	   /// </summary>
	   /// <param name="hours"> the hour of the day (0-23) </param>
	   /// <param name="minutes"> the minute setting (0-59) </param>
	   /// <param name="seconds"> the second setting (0-59) </param>
	   /// <param name="day"> the day of the week (1-7, 1==Sunday) </param>
	   /// <param name="alarmFrequency"> frequency that the alarm should occur at </param>
	   /// <param name="state"> current state of the device returned from <code>readDevice()</code>
	   /// </param>
	   /// <seealso cref= #readDevice() </seealso>
	   /// <seealso cref= #writeDevice(byte[]) </seealso>
	   /// <seealso cref= #getClockAlarm(byte[]) </seealso>
	   /// <seealso cref= #ONCE_PER_SECOND </seealso>
	   /// <seealso cref= #ONCE_PER_MINUTE </seealso>
	   /// <seealso cref= #ONCE_PER_HOUR </seealso>
	   /// <seealso cref= #ONCE_PER_DAY </seealso>
	   /// <seealso cref= #ONCE_PER_WEEK </seealso>
	   public virtual void setClockAlarm(int hours, int minutes, int seconds, int day, int alarmFrequency, byte[] state)
	   {
		  setTime(0x207, hours, minutes, seconds, false, state);

		  state [0x0a] = (byte) day;

		  int number_0_msb = 0; //how many of the MS, MM, MH, MD bytes have

		  //0 as their ms bit???
		  switch (alarmFrequency)
		  {
			 case ONCE_PER_SECOND :
				number_0_msb = 0;
				break;
			 case ONCE_PER_MINUTE :
				number_0_msb = 1;
				break;
			 case ONCE_PER_HOUR :
				number_0_msb = 2;
				break;
			 case ONCE_PER_DAY :
				number_0_msb = 3;
				break;
			 default:
				 goto case ONCE_PER_WEEK;
			 case ONCE_PER_WEEK :
				number_0_msb = 4;
				break;
		  }

		  for (int i = 0x07; i < 0x0b; i++)
		  {
			 if (number_0_msb > 0)
			 {
				number_0_msb--;

				state [i] = (byte)(state [i] & 0x7f); //make the leading bit 0
			 }
			 else
			 {
				state [i] = (byte)(state [i] | 0x80); //make the laeding bit 1
			 }
		  }
	   }

	   /// <summary>
	   /// Returns the rate at which the DS1921 takes temperature samples.
	   /// This rate is set when the mission is enabled (in the method
	   /// <code>enableMission(int)</code>.
	   /// </summary>
	   /// <param name="state"> current state of the device returned from <code>readDevice()</code>
	   /// </param>
	   /// <returns> the time, in minutes, between temperature readings
	   /// </returns>
	   /// <seealso cref= #enableMission(int) </seealso>
	   /// <seealso cref= #readDevice() </seealso>
	   public virtual int getSampleRate(byte[] state)
	   {
		  return (int)(0x0FF & state [0x0D]);
	   }

	   /// <summary>
	   /// Determines the number of samples taken on this mission.
	   /// Only the last 2048 samples appear in the Thermocron's log,
	   /// though all readings from the current mission are logged
	   /// in the histogram.
	   /// </summary>
	   /// <param name="state"> current state of the device returned from <code>readDevice()</code>
	   /// </param>
	   /// <returns> the number of samples taken in the current mission
	   /// </returns>
	   /// <seealso cref= #readDevice() </seealso>
	   /// <seealso cref= #getDeviceSamplesCounter(byte[]) </seealso>
	   public virtual int getMissionSamplesCounter(byte[] state)
	   {
		  byte low = state [0x1A];
		  byte medium = state [0x1B];
		  byte high = state [0x1C];

		  return (((high << 16) & 0x00ff0000) | ((medium << 8) & 0x0000ff00) | (low & 0x000000ff));
	   }

	   /// <summary>
	   /// <para>Determines the total number of samples taken by this Thermocron.
	   /// This includes samples taken in past missions.  It also includes
	   /// 'forced' readings.  A 'forced' reading refers to a reading taken
	   /// when the Thermocron does not have a running mission and is instructed
	   /// to read the current temperature.</para>
	   /// 
	   /// <para>The DS1921 Thermocron is tested to last for 1 million temperature
	   /// readings.</para>
	   /// </summary>
	   /// <param name="state"> current state of the device returned from <code>readDevice()</code>
	   /// </param>
	   /// <returns> the total number of measurements taken by this Thermocron
	   /// </returns>
	   /// <seealso cref= #readDevice() </seealso>
	   /// <seealso cref= #getMissionSamplesCounter(byte[]) </seealso>
	   public virtual int getDeviceSamplesCounter(byte[] state)
	   {
		  byte low = state [0x1D];
		  byte medium = state [0x1E];
		  byte high = state [0x1F];

		  return (((high << 16) & 0x00ff0000) | ((medium << 8) & 0x0000ff00) | (low & 0x000000ff));
	   }

	   /// <summary>
	   /// Returns the date and time that the last mission was
	   /// started.  The values in the <code>java.util.Calendar</code>
	   /// object are fully specified.  In other words, the year, month,
	   /// date, hour, minute, and second are all valid in the returned
	   /// object.
	   /// </summary>
	   /// <param name="state"> current state of the device returned from <code>readDevice()</code>
	   /// </param>
	   /// <returns> the date and time that the last mission was started
	   /// </returns>
	   /// <seealso cref= #readDevice() </seealso>
	   public virtual DateTime getMissionTimeStamp(byte[] state)
	   {
		  /* i know here that the mission time stamp does not start at address 214,
		   * however--the mission time stamp starts with minutes, and i have
		   * a method to read the seconds.  since i can ignore that in this case,
		   * i can go ahead and 'fake' read the seconds
		   */
		  int[] time_result = getTime(0x214, state);
		  int[] date_result = getDate(0x217, state);
		  int year = date_result [0] % 100;

		  // determine the century based on the number of samples taken
		  int numberOfCounts = getMissionSamplesCounter(state);
		  int timeBetweenCounts = getSampleRate(state);
		  int yearsSinceMissionStart = (int)((numberOfCounts * timeBetweenCounts) / (525600));

		  // get a rough estimate of how long ago this was
		  //result = getDateTime(state);
		  int[] offset_result = getDate(0x204, state);
		  int result_year = offset_result [0];

		  // add the century based on this calculation
		  //if ((result.get(Calendar.YEAR) - yearsSinceMissionStart) > 1999)
		  if ((result_year - yearsSinceMissionStart) > 1999)
		  {
			 year += 2000;
		  }
		  else
		  {
			 year += 1900;
		  }

		  // protect against deviations that may cause gross errors
		  //if (year > result.get(Calendar.YEAR))
		  if (year > result_year)
		  {
			 year -= 100;
		  }

		  DateTime result = new DateTime(
              year, 
              date_result[1], 
              date_result[2],
              time_result[2],
              time_result[1],
              0);

		  return result;
	   }

	   /// <summary>
	   /// <para>Helps determine the times for values in a temperature log.  If rollover
	   /// is enabled, temperature log entries will over-write previous
	   /// entries once more than 2048 logs are written.  The returned value can be
	   /// added to the underlying millisecond value of <code>getMissionTimeStamp()</code>
	   /// to determine the time that the 'first' log entry actually occurred.</para>
	   /// <pre><code>
	   ///      //ds1921 is a OneWireContainer21
	   ///      byte[] state = ds1921.readDevice();
	   ///      Calendar c = ds1921.getMissionTimeStamp(state);
	   ///      //find the time for the first log entry
	   ///      long first_entry = c.getTime().getTime();
	   ///      first_entry += ds1921.getFirstLogOffset(state);
	   ///      . . .
	   /// </code></pre>
	   /// 
	   /// <para>Be cautious of Java's Daylight Savings Time offsets when using this
	   /// function--if you use the <code>Date</code> or <code>Calendar</code>
	   /// class to print this out, Java may try to automatically format
	   /// the <code>java.lang.String</code> to handle Daylight Savings Time, resulting in offset
	   /// by 1 hour problems.</para>
	   /// </summary>
	   /// <param name="state"> current state of the device returned from <code>readDevice()</code>
	   /// </param>
	   /// <returns> milliseconds between the beginning of the mission
	   ///     and the time of the first log entry reported from
	   ///     <code>getTemperatureLog()</code>
	   /// </returns>
	   /// <seealso cref= #readDevice() </seealso>
	   /// <seealso cref= #getMissionTimeStamp(byte[]) </seealso>
	   /// <seealso cref= #getTemperatureLog(byte[]) </seealso>
	   public virtual long getFirstLogOffset(byte[] state)
	   {
		  long counter = getMissionSamplesCounter(state);


		  if ((counter < 2049) || (!getFlag(CONTROL_REGISTER, ROLLOVER_ENABLE_FLAG, state)))
		  {
			 return 0;
		  }

		  //else we need to figure out when the first sample occurred
		  //since there are counter entries, the first entry is (counter - 2048)
		  //so if we multiply that times milliseconds between entry,
		  //we should be OK
		  counter -= 2048;

		  //rate is the rate in minutes, must multiply by 60 to be seconds,
		  //then by 1000 to be milliseconds
		  int rate = this.getSampleRate(state);

		  counter = counter * rate * 1000 * 60;

		  return counter;
	   }

	   /// <summary>
	   /// <para>Returns the log of temperature measurements.  Each <code>byte</code>
	   /// in the returned array is an independent sample.  Use the method
	   /// <code>decodeTemperature(byte)</code> to get the double value
	   /// of the encoded temperature.  See the DS1921 datasheet for more
	   /// on the data's encoding scheme.  The array's length equals the
	   /// number of measurements taken thus far.  The temperature log can
	   /// be read while a mission is still in progress. </para>
	   /// 
	   /// <para>Note that although this method takes the device state as a parameter,
	   /// this method still must communicate directly with the Thermocron
	   /// to read the log.</para>
	   /// </summary>
	   /// <param name="state"> current state of the device returned from <code>readDevice()</code>
	   /// </param>
	   /// <returns> the DS1921's encoded temperature log
	   /// </returns>
	   /// <exception cref="OneWireIOException"> on a 1-Wire communication error such as
	   ///         reading an incorrect CRC from a 1-Wire device.  This could be
	   ///         caused by a physical interruption in the 1-Wire Network due to
	   ///         shorts or a newly arriving 1-Wire device issuing a 'presence pulse'. </exception>
	   /// <exception cref="OneWireException"> on a communication or setup error with the 1-Wire
	   ///         adapter
	   /// </exception>
	   /// <seealso cref= #decodeTemperature(byte) </seealso>
	   /// <seealso cref= #readDevice() </seealso>
	   /// <seealso cref= #getFirstLogOffset(byte[]) </seealso>
	   /// <seealso cref= #getMissionTimeStamp(byte[]) </seealso>
	   public virtual byte[] getTemperatureLog(byte[] state)
	   {
		   lock (this)
		   {
			  byte[] result;
        
			  /* get the number of samples and the rate at which they were taken */
			  int numberOfReadings = getMissionSamplesCounter(state);
        
			  // used for rollover
			  int offsetDepth = 0;
        
			  /* this next line checks the rollover bit and whether a rollover occurred */
			  if ((getFlag(CONTROL_REGISTER, ROLLOVER_ENABLE_FLAG, state)) && (numberOfReadings > 2048))
			  {
        
				 // offsetDepth holds the number of new readings before we hit older ones
				 offsetDepth = numberOfReadings % 2048;
			  }
        
			  // the max number of readings STORED is 2048
			  if (numberOfReadings > 2048)
			  {
				 numberOfReadings = 2048;
			  }
        
			  result = new byte [numberOfReadings];
        
			  int offset = 0;
        
			  while (offset < numberOfReadings)
			  {
				 log.readPageCRC(offset >> 5, false, read_log_buffer, offset);
        
				 offset += 32;
			  }
        
			  //put the bytes into the output array, but careful for the case
			  //where we rolled over that we start in the right place!
			  Array.Copy(read_log_buffer, offsetDepth, result, 0, numberOfReadings - offsetDepth);
			  Array.Copy(read_log_buffer, 0, result, numberOfReadings - offsetDepth, offsetDepth);
        
			  return result;
		   }
	   }

	   /// <summary>
	   /// <para>Returns an array of at most 64 counter bins holding the DS1921 histogram data
	   /// (63 bins for the DS1921L-F5X and 64 bins for the DS1921H or DS1921Z).  For the
	   /// temperature histogram, the DS1921 provides bins that each consist of a 16-bit,
	   /// non rolling-over binary counter that is incremented each time a temperature value
	   /// acquired during a mission falls into the range of the bin. The bin to be
	   /// updated is determined by cutting off the two least significant bits of the
	   /// binary temperature value.  For example, on a DS1921L-F5X, bin 0 will hold the
	   /// counter for temperatures ranging from -40 to -38.5 (Celsius) and lower. Bin 1
	   /// is associated with the range of -38 to 36.5 and so on.  The last bin, in this
	   /// case bin 62, holds temperature values of 84 degrees and higher.  Please see the
	   /// respective DS1921H or DS1921Z datasheets for their bin arrangements.  The
	   /// temperature histogram can be read while a mission is still in progress.</para>
	   /// </summary>
	   /// <returns> the 63 temperature counters
	   /// </returns>
	   /// <exception cref="OneWireIOException"> on a 1-Wire communication error such as
	   ///         reading an incorrect CRC from a 1-Wire device.  This could be
	   ///         caused by a physical interruption in the 1-Wire Network due to
	   ///         shorts or a newly arriving 1-Wire device issuing a 'presence pulse'. </exception>
	   /// <exception cref="OneWireException"> on a communication or setup error with the 1-Wire
	   ///         adapter </exception>
	   public virtual int[] TemperatureHistogram
	   {
		   get
		   {
			  int[] result;
			  if (isDS1921HZ)
			  {
				 result = new int[64]; //  One more bin for the H or Z
			  }
			  else
			  {
				 result = new int [63];
			  }
			  byte[] buffer = new byte [128];
    
			  /* read the data first */
			  int offset = 0;
    
			  while (offset < 128)
			  {
				 histogram.readPageCRC(offset >> 5, false, buffer, offset);
    
				 offset += 32;
			  }
    
			  int i = 0, j = 0;
    
			  while (i < result.Length)
			  {
    
				 // get the 2 byte counter values
				 result [i] = (buffer [j] & 0x00ff) | ((buffer [j + 1] << 8) & 0xff00);
    
				 i++;
    
				 j += 2;
			  }
    
			  return result;
		   }
	   }

	   /// <summary>
	   /// Returns <code>true</code> if the specified alarm has been
	   /// triggered.  Valid values for the <code>alarmBit</code>
	   /// parameter are:<code><pre>
	   ///     TEMPERATURE_LOW_ALARM
	   ///     TEMPERATURE_HIGH_ALARM
	   ///     TIMER_ALARM
	   /// </pre></code>
	   /// </summary>
	   /// <param name="alarmBit"> the alarm to check </param>
	   /// <param name="state"> current state of the device returned from <code>readDevice()</code>
	   /// </param>
	   /// <returns> <true> if the specified alarm has been triggered
	   /// </returns>
	   /// <seealso cref= #TEMPERATURE_LOW_ALARM </seealso>
	   /// <seealso cref= #TEMPERATURE_HIGH_ALARM </seealso>
	   /// <seealso cref= #TIMER_ALARM </seealso>
	   /// <seealso cref= #readDevice() </seealso>
	   /// <seealso cref= #getAlarmHistory(byte) </seealso>
	   public virtual bool getAlarmStatus(byte alarmBit, byte[] state)
	   {
		  return ((state [STATUS_REGISTER & 31] & alarmBit) != 0);
	   }

	   /// <summary>
	   /// <para>Returns an array containing the alarm log.
	   /// The DS1921 contains two separate alarm logs. One for the high temperature
	   /// alarm and one for the low temperature alarm.  Each log can store
	   /// up to 12 log entries and each log entry will record up to 255
	   /// consecutive alarm violations.</para>
	   /// 
	   /// <para>The returned array is not altered from its representation
	   /// on the DS1921 Thermocron.  It is therefore up to the caller to
	   /// interpret the data.  The number of logs in this alarm history
	   /// is equal to the array length divided by 4, since each entry
	   /// is 4 bytes.  The first three bytes are the number of samples into the
	   /// mission that the alarm occurred.  The fourth byte is the number of
	   /// consecutive samples that violated the alarm.  To extract the starting
	   /// offset and number of violations from the array:</para>
	   /// <code><pre>
	   ///       byte[] data = ds1921.getAlarmHistory(OneWireContainer21.TEMPERATURE_HIGH_ALARM);
	   ///       int start_offset;
	   ///       int violation_count;
	   ///       . . .
	   ///       for (int i=0;i < data.length/4; i++)
	   ///       {
	   ///           start_offset = (data [i * 4] & 0x0ff)
	   ///                     | ((data [i * 4 + 1] << 8) & 0x0ff00)
	   ///                     | ((data [i * 4 + 2] << 16) & 0x0ff0000);
	   ///           violation_count = 0x0ff & data[i*4+3];
	   /// 
	   ///           . . .
	   /// 
	   ///           // note: you may find it useful to multiply start_offset
	   ///           //       by getSampleRate() in order to get the number of
	   ///           //       minutes into the mission that the violation occurred
	   ///           //       on.  You can do the same with violation_count
	   ///           //       to determine how long the violation lasted.
	   ///       }
	   /// </pre></code>
	   /// 
	   /// <para>Acceptable values for the <code>alarmBit</code>
	   /// parameter are:<pre><code>
	   ///     TEMPERATURE_LOW_ALARM
	   ///     TEMPERATURE_HIGH_ALARM</code></pre>
	   /// </para>
	   /// </summary>
	   /// <param name="alarmBit"> the alarm log to get
	   /// </param>
	   /// <returns> the time/duration of the alarm (see above for the structure of the array)
	   /// </returns>
	   /// <exception cref="OneWireIOException"> on a 1-Wire communication error such as
	   ///         reading an incorrect CRC from a 1-Wire device.  This could be
	   ///         caused by a physical interruption in the 1-Wire Network due to
	   ///         shorts or a newly arriving 1-Wire device issuing a 'presence pulse'. </exception>
	   /// <exception cref="OneWireException"> on a communication or setup error with the 1-Wire
	   ///         adapter
	   /// </exception>
	   /// <seealso cref= #getAlarmStatus(byte,byte[]) </seealso>
	   public virtual byte[] getAlarmHistory(byte alarmBit)
	   {
		  int counter = 0;
		  byte[] temp_data = new byte [96];
		  int offset = 0;

		  while (offset < 96)
		  {
			 alarm.readPageCRC(offset >> 5, false, temp_data, offset);

			 offset += 32;
		  }

		  if (alarmBit == TEMPERATURE_LOW_ALARM)
		  {
			 offset = 0;
		  }
		  else
		  {
			 offset = 48;
		  }

		  /* check how many entries there are (each entry consists of 4 bytes)
		     the fourth byte of each entry is the counter - check if its > 0 */
		  /* but there can only be a maximum of 12 entries! */
		  while (counter < 12 && (counter * 4 + 3 + offset < temp_data.Length) && (temp_data [counter * 4 + 3 + offset] != 0))
		  {
			 counter++;
		  }

		  byte[] data = new byte [counter << 2];

		  Array.Copy(temp_data, offset, data, 0, counter << 2);

		  return data;
	   }

	   /// <summary>
	   /// Retrieves the 1-Wire device sensor state.  This state is
	   /// returned as a byte array.  Pass this byte array to the 'get'
	   /// and 'set' methods.  If the device state needs to be changed then call
	   /// the 'writeDevice' to finalize the changes.
	   /// </summary>
	   /// <returns> 1-Wire device sensor state
	   /// </returns>
	   /// <exception cref="OneWireIOException"> on a 1-Wire communication error such as
	   ///         reading an incorrect CRC from a 1-Wire device.  This could be
	   ///         caused by a physical interruption in the 1-Wire Network due to
	   ///         shorts or a newly arriving 1-Wire device issuing a 'presence pulse'. </exception>
	   /// <exception cref="OneWireException"> on a communication or setup error with the 1-Wire
	   ///         adapter </exception>
	   public virtual byte[] readDevice()
	   {
		  byte[] buffer = new byte [32];

		  //going to return the register page, 32 bytes
		  register.readPageCRC(0, false, buffer, 0);

		  return buffer;
	   }

	   /// <summary>
	   /// Writes the 1-Wire device sensor state that
	   /// have been changed by 'set' methods.  Only the state registers that
	   /// changed are updated.  This is done by referencing a field information
	   /// appended to the state data.
	   /// </summary>
	   /// <param name="state"> 1-Wire device sensor state
	   /// </param>
	   /// <exception cref="OneWireIOException"> on a 1-Wire communication error such as
	   ///         reading an incorrect CRC from a 1-Wire device.  This could be
	   ///         caused by a physical interruption in the 1-Wire Network due to
	   ///         shorts or a newly arriving 1-Wire device issuing a 'presence pulse'. </exception>
	   /// <exception cref="OneWireException"> on a communication or setup error with the 1-Wire
	   ///         adapter </exception>
	   public virtual void writeDevice(byte[] state)
	   {
		  if (getFlag(STATUS_REGISTER, MISSION_IN_PROGRESS_FLAG))
		  {
			 throw new OneWireIOException("OneWireContainer21-Cannot write to registers while mission is in progress.");
		  }

		  int start = updatertc ? 0 : 7;

		  register.write(start, state, start, 20 - start); //last 12 bytes are read only

		  lock (this)
		  {
			 updatertc = false;
		  }
	   }

	   ////////////////////////////////////////////////////////////////////////////////////////
	   //
	   //       Temperature Interface Functions
	   //
	   ////////////////////////////////////////////////////////////////////////////////////////

	   /// <summary>
	   /// Checks to see if this temperature measuring device has high/low
	   /// trip alarms.
	   /// </summary>
	   /// <returns> <code>true</code> if this <code>TemperatureContainer</code>
	   ///         has high/low trip alarms
	   /// </returns>
	   /// <seealso cref=    #getTemperatureAlarm </seealso>
	   /// <seealso cref=    #setTemperatureAlarm </seealso>
	   public virtual bool hasTemperatureAlarms()
	   {
		  return true;
	   }

	   /// <summary>
	   /// Checks to see if this device has selectable temperature resolution.
	   /// </summary>
	   /// <returns> <code>true</code> if this <code>TemperatureContainer</code>
	   ///         has selectable temperature resolution
	   /// </returns>
	   /// <seealso cref=    #getTemperatureResolution </seealso>
	   /// <seealso cref=    #getTemperatureResolutions </seealso>
	   /// <seealso cref=    #setTemperatureResolution </seealso>
	   public virtual bool hasSelectableTemperatureResolution()
	   {
		  return false;
	   }

	   /// <summary>
	   /// Get an array of available temperature resolutions in Celsius.
	   /// </summary>
	   /// <returns> byte array of available temperature resolutions in Celsius with
	   ///         minimum resolution as the first element and maximum resolution
	   ///         as the last element
	   /// </returns>
	   /// <seealso cref=    #hasSelectableTemperatureResolution </seealso>
	   /// <seealso cref=    #getTemperatureResolution </seealso>
	   /// <seealso cref=    #setTemperatureResolution </seealso>
	   public virtual double[] TemperatureResolutions
	   {
		   get
		   {
			  double[] d = new double [1];
    
			  d [0] = temperatureResolution;
    
			  return d;
		   }
	   }

	   /// <summary>
	   /// Gets the temperature alarm resolution in Celsius.
	   /// </summary>
	   /// <returns> temperature alarm resolution in Celsius for this 1-wire device
	   /// </returns>
	   /// <seealso cref=    #hasTemperatureAlarms </seealso>
	   /// <seealso cref=    #getTemperatureAlarm </seealso>
	   /// <seealso cref=    #setTemperatureAlarm
	   ///  </seealso>
	   public virtual double TemperatureAlarmResolution
	   {
		   get
		   {
			  return 1.5;
		   }
	   }

	   /// <summary>
	   /// Gets the maximum temperature in Celsius.
	   /// </summary>
	   /// <returns> maximum temperature in Celsius for this 1-wire device
	   /// </returns>
	   /// <seealso cref= #getMinTemperature() </seealso>
	   public virtual double MaxTemperature
	   {
		   get
		   {
			  return OperatingRangeHighTemperature;
		   }
	   }

	   /// <summary>
	   /// Gets the minimum temperature in Celsius.
	   /// </summary>
	   /// <returns> minimum temperature in Celsius for this 1-wire device
	   /// </returns>
	   /// <seealso cref= #getMaxTemperature() </seealso>
	   public virtual double MinTemperature
	   {
		   get
		   {
			  return OperatingRangeLowTemperature;
		   }
	   }

	   //--------
	   //-------- Temperature I/O Methods
	   //--------

	   /// <summary>
	   /// Performs a temperature conversion.  Use the <code>state</code>
	   /// information to calculate the conversion time.
	   /// </summary>
	   /// <param name="state"> byte array with device state information
	   /// </param>
	   /// <exception cref="OneWireIOException"> on a 1-Wire communication error such as
	   ///         reading an incorrect CRC from a 1-Wire device.  This could be
	   ///         caused by a physical interruption in the 1-Wire Network due to
	   ///         shorts or a newly arriving 1-Wire device issuing a 'presence pulse'.
	   ///         In the case of the DS1921 Thermocron, this could also be due to a
	   ///         currently running mission. </exception>
	   /// <exception cref="OneWireException"> on a communication or setup error with the 1-Wire
	   ///         adapter </exception>
	   public virtual void doTemperatureConvert(byte[] state)
	   {

		  /* check for mission in progress */
		  if (getFlag(STATUS_REGISTER, MISSION_IN_PROGRESS_FLAG))
		  {
			 throw new OneWireIOException("OneWireContainer21-Cant force " + "temperature read during a mission.");
		  }

		  /* get the temperature*/
		  if (doSpeedEnable)
		  {
			 doSpeed(); //we aren't worried about how long this takes...we're sleeping for 750 ms!
		  }

		  adapter.reset();

		  if (adapter.select(address))
		  {

			 // perform the temperature conversion
			 adapter.putByte(CONVERT_TEMPERATURE_COMMAND);

			 try
			 {
				Thread.Sleep(750);
			 }
			 catch (InterruptedException)
			 {
			 }

			 // grab the temperature
			 state [0x11] = readByte(0x211);
		  }
		  else
		  {
			 throw new OneWireException("OneWireContainer21-Device not found!");
		  }
	   }

	   //--------
	   //-------- Temperature 'get' Methods
	   //--------

	   /// <summary>
	   /// Gets the temperature value in Celsius from the <code>state</code>
	   /// data retrieved from the <code>readDevice()</code> method.
	   /// </summary>
	   /// <param name="state"> byte array with device state information
	   /// </param>
	   /// <returns> temperature in Celsius from the last
	   ///                     <code>doTemperatureConvert()</code> </returns>
	   public virtual double getTemperature(byte[] state)
	   {
		  return decodeTemperature(state [0x11]);
	   }

	   /// <summary>
	   /// Gets the specified temperature alarm value in Celsius from the
	   /// <code>state</code> data retrieved from the
	   /// <code>readDevice()</code> method.
	   /// </summary>
	   /// <param name="alarmType"> valid value: <code>ALARM_HIGH</code> or
	   ///                   <code>ALARM_LOW</code> </param>
	   /// <param name="state">     byte array with device state information
	   /// </param>
	   /// <returns> temperature alarm trip values in Celsius for this 1-wire device
	   /// </returns>
	   /// <seealso cref=    #hasTemperatureAlarms </seealso>
	   /// <seealso cref=    #setTemperatureAlarm </seealso>
	   public virtual double getTemperatureAlarm(int alarmType, byte[] state)
	   {
		  if ((alarmType == TEMPERATURE_HIGH_ALARM) || (alarmType == TemperatureContainer_Fields.ALARM_HIGH))
		  {
			 return decodeTemperature(state [0x0c]);
		  }
		  else
		  {
			 return decodeTemperature(state [0x0b]);
		  }
	   }

	   /// <summary>
	   /// Gets the current temperature resolution in Celsius from the
	   /// <code>state</code> data retrieved from the <code>readDevice()</code>
	   /// method.
	   /// </summary>
	   /// <param name="state"> byte array with device state information
	   /// </param>
	   /// <returns> temperature resolution in Celsius for this 1-wire device
	   /// </returns>
	   /// <seealso cref=    #hasSelectableTemperatureResolution </seealso>
	   /// <seealso cref=    #getTemperatureResolutions </seealso>
	   /// <seealso cref=    #setTemperatureResolution </seealso>
	   public virtual double getTemperatureResolution(byte[] state)
	   {
		  return temperatureResolution;
	   }

	   //--------
	   //-------- Temperature 'set' Methods
	   //--------

	   /// <summary>
	   /// Sets the temperature alarm value in Celsius in the provided
	   /// <code>state</code> data.
	   /// Use the method <code>writeDevice()</code> with
	   /// this data to finalize the change to the device.
	   /// </summary>
	   /// <param name="alarmType">  valid value: <code>ALARM_HIGH</code> or
	   ///                    <code>ALARM_LOW</code> </param>
	   /// <param name="alarmValue"> alarm trip value in Celsius </param>
	   /// <param name="state">      byte array with device state information
	   /// </param>
	   /// <seealso cref=    #hasTemperatureAlarms </seealso>
	   /// <seealso cref=    #getTemperatureAlarm </seealso>
	   public virtual void setTemperatureAlarm(int alarmType, double alarmValue, byte[] state)
	   {
		  double histogramLow = HistogramLowTemperature;
		  double histogramHigh = PhysicalRangeHighTemperature + (HistogramBinWidth - TemperatureResolution);
		  byte alarm = encodeTemperature(alarmValue);

		  // take special care of top and bottom of temperature ranges for the different
		  // types of thermochrons.
		  if (isDS1921HZ)
		  {
			 if (alarmValue < histogramLow)
			 {
				alarm = 0;
			 }

			 if (alarmValue > histogramHigh)
			 {
				alarm = 0xFF; // maximum value stand for the histogram high temperature
			 }
		  }
		  else
		  {
			 if (alarmValue < -40.0)
			 {
				alarm = 0;
			 }

			 if (alarmValue > 85.0)
			 {
				alarm = 0xfa; // maximum value stands for 85.0 C
			 }
		  }

		  if ((alarmType == TEMPERATURE_HIGH_ALARM) || (alarmType == TemperatureContainer_Fields.ALARM_HIGH))
		  {
			 state [0x0c] = alarm;
		  }
		  else
		  {
			 state [0x0b] = alarm;
		  }
	   }

	   /// <summary>
	   /// Sets the current temperature resolution in Celsius in the provided
	   /// <code>state</code> data.   Use the method <code>writeDevice()</code>
	   /// with this data to finalize the change to the device.
	   /// </summary>
	   /// <param name="resolution"> temperature resolution in Celsius </param>
	   /// <param name="state">      byte array with device state information
	   /// </param>
	   /// <exception cref="OneWireException"> if the device does not support
	   /// selectable temperature resolution
	   /// </exception>
	   /// <seealso cref=    #hasSelectableTemperatureResolution </seealso>
	   /// <seealso cref=    #getTemperatureResolution </seealso>
	   /// <seealso cref=    #getTemperatureResolutions </seealso>
	   public virtual void setTemperatureResolution(double resolution, byte[] state)
	   {
		  throw new OneWireException("Selectable Temperature Resolution Not Supported");
	   }

	   ////////////////////////////////////////////////////////////////////////////////////////
	   //
	   //       Clock Interface Functions
	   //
	   ////////////////////////////////////////////////////////////////////////////////////////

	   /// <summary>
	   /// Checks to see if the clock has an alarm feature.
	   /// </summary>
	   /// <returns> true if the Real-Time clock has an alarm
	   /// </returns>
	   /// <seealso cref= #getClockAlarm(byte[]) </seealso>
	   /// <seealso cref= #isClockAlarmEnabled(byte[]) </seealso>
	   /// <seealso cref= #isClockAlarming(byte[]) </seealso>
	   /// <seealso cref= #setClockAlarm(long,byte[]) </seealso>
	   /// <seealso cref= #setClockAlarmEnable(bool,byte[]) </seealso>
	   public virtual bool hasClockAlarm()
	   {
		  return true;
	   }

	   /// <summary>
	   /// Checks to see if the clock can be disabled.
	   /// </summary>
	   /// <returns> true if the clock can be enabled and disabled
	   /// </returns>
	   /// <seealso cref= #isClockRunning(byte[]) </seealso>
	   /// <seealso cref= #setClockRunEnable(bool,byte[]) </seealso>
	   public virtual bool canDisableClock()
	   {
		  return true;
	   }

	   /// <summary>
	   /// Gets the clock resolution in milliseconds
	   /// </summary>
	   /// <returns> the clock resolution in milliseconds </returns>
	   public virtual long ClockResolution
	   {
		   get
		   {
			  return 1000;
		   }
	   }

	   //--------
	   //-------- Clock 'get' Methods
	   //--------

	   /// <summary>
	   /// Extracts the Real-Time clock value in milliseconds.
	   /// </summary>
	   /// <param name="state"> current state of the device returned from <code>readDevice()</code>
	   /// </param>
	   /// <returns> the time represented in this clock in milliseconds since 1970
	   /// </returns>
	   /// <seealso cref= com.dalsemi.onewire.container.OneWireSensor#readDevice() </seealso>
	   /// <seealso cref= #setClock(long,byte[]) </seealso>
	   public virtual long getClock(byte[] state)
	   {

		  /* grab the time (at location 200, date at 204) */
		  int[] time = getTime(0x200, state);
		  int[] date = getDate(0x204, state);

          //date[1] - 1 because Java months are 0 offset
          //date[0] - 1900 because Java years are from 1900
          //Date d = new Date(date[0]-1900, date[1]-1, date[2], time[2], time[1], time[0]);
          DateTime result = new DateTime(date[0], date[1], date[2], time[2], time[1], time[0]);
          //result.set(DateTime.YEAR, date[0]);
		  //result.set(DateTime.MONTH, date[1] - 1);
		  //result.set(DateTime.DATE, date[2]);
		  //result.set(DateTime.HOUR_OF_DAY, time[2]);
		  //result.set(DateTime.MINUTE, time[1]);
		  //result.set(DateTime.SECOND, time[0]);

          TimeSpan t = new TimeSpan(result.Ticks);
		  return (long)t.TotalMilliseconds;
	   }

	   /// <summary>
	   /// Extracts the clock alarm value for the Real-Time clock.  In the case
	   /// of the DS1921 Thermocron, this is the time that the next periodic
	   /// alarm event will occur.
	   /// </summary>
	   /// <param name="state"> current state of the device returned from <code>readDevice()</code>
	   /// </param>
	   /// <returns> milliseconds since 1970 that the clock alarm is set to
	   /// </returns>
	   /// <seealso cref= com.dalsemi.onewire.container.OneWireSensor#readDevice() </seealso>
	   /// <seealso cref= #hasClockAlarm() </seealso>
	   /// <seealso cref= #isClockAlarmEnabled(byte[]) </seealso>
	   /// <seealso cref= #isClockAlarming(byte[]) </seealso>
	   /// <seealso cref= #setClockAlarm(long,byte[]) </seealso>
	   /// <seealso cref= #setClockAlarmEnable(bool,byte[]) </seealso>
	   public virtual long getClockAlarm(byte[] state)
	   {
          TimeSpan t;

          //first get the normal real time clock
          int[] time = getTime(0x200, state);
		  int[] date = getDate(0x204, state);

		  //date[0] = year
		  //date[1] = month
		  //date[2] = date
		  //time[2] = hour
		  //time[1] = minute
		  //time[0] = second
		  DateTime c = new DateTime(date[0], date[1], date[2], time[2], time[1], time[0]);

		  //get the seconds into the day we are at
		  int time_into_day = time [0] + 60 * time [1] + 60 * 60 * time [2];

		  //now lets get the alarm specs
		  int[] a_time = getTime(0x207, state);

		  //get the seconds into the day the alarm is at
		  int a_time_into_day = a_time [0] + 60 * a_time [1] + 60 * 60 * a_time [2];

		  // now put the day of the week in there
		  byte dayOfWeek = (byte)(state [0x0A] & 0x07);

		  if (dayOfWeek == 0)
		  {
			 dayOfWeek++;
		  }

		  byte MS = (byte)(((int)((uint)state [0x07] >> 7)) & 0x01);
		  byte MM = (byte)(((int)((uint)state [0x08] >> 7)) & 0x01);
		  byte MH = (byte)(((int)((uint)state [0x09] >> 7)) & 0x01);
		  byte MD = (byte)(((int)((uint)state [0x0A] >> 7)) & 0x01);

		  long temp_time = 0;
		  int MILLIS_PER_DAY = 1000 * 60 * 60 * 24;

		  switch (MS + MM + MH + MD)
		  {
			 case 4: //ONCE_PER_SECOND
				c.AddSeconds(1);
				break;
			 case 3: //ONCE_PER_MINUTE
				if (!(a_time_into_day < time_into_day)) //alarm has occurred
				{
				   c.AddMinutes(1);
				}
                DateTime ts = new DateTime(c.Year,c.Month,c.Day,c.Hour,c.Minute, a_time[0]);
                c = ts;
				break;
			 case 2: //ONCE_PER_HOUR
				if (!(a_time_into_day < time_into_day)) //alarm has occurred
				{
				   c.AddHours(1); //will occur again next hour
				}

                DateTime th = new DateTime(c.Year,c.Month,c.Day,c.Hour, a_time[1], a_time[0]);
                c = th;
                break;
			 case 1: //ONCE_PER_DAY
                DateTime td = new DateTime(c.Year, c.Month, c.Day, a_time[2], a_time[1], a_time[0]);
                c = td;

				if ((a_time_into_day < time_into_day)) //alarm has occurred
				{
				   c.AddDays(1); //will occur again tomorrow
				}
				break;
			 default:
				 goto case 0;
			 case 0: //ONCE_PER_WEEK
                DateTime tw = new DateTime(c.Year, c.Month, c.Day, a_time[2], a_time[1], a_time[0]);
                c = tw;

				// c.set(c.AM_PM, (a_time[2] > 11) ? c.PM : c.AM);

                t = new TimeSpan(c.Ticks);
				temp_time = (long)t.TotalMilliseconds;

				if (dayOfWeek == (byte)c.DayOfWeek)
				{

				   //has alarm already occurred today?
				   if ((a_time_into_day < time_into_day)) //alarm has occurred
				   {
					  temp_time += (7 * MILLIS_PER_DAY); //will occur again next week
				   }
				}
				else
				{

				   //roll the day of the week until it matches
				   int cdayofweek = (int)c.DayOfWeek;

				   while ((dayOfWeek % 7) != (cdayofweek++ % 7))
				   {
					   temp_time += MILLIS_PER_DAY;
				   }
					  //c.roll(c.DATE, true);
				}

				return temp_time;
		  }

          t = new TimeSpan(c.Ticks);
		  return (long)t.TotalMilliseconds; //c->getTime returns Date, Date->getTime returns long
	   }

	   /// <summary>
	   /// Checks if the clock alarm flag has been set.
	   /// This will occur when the value of the Real-Time
	   /// clock equals the value of the clock alarm.
	   /// </summary>
	   /// <param name="state"> current state of the device returned from <code>readDevice()</code>
	   /// </param>
	   /// <returns> true if the Real-Time clock is alarming
	   /// </returns>
	   /// <seealso cref= com.dalsemi.onewire.container.OneWireSensor#readDevice() </seealso>
	   /// <seealso cref= #hasClockAlarm() </seealso>
	   /// <seealso cref= #isClockAlarmEnabled(byte[]) </seealso>
	   /// <seealso cref= #getClockAlarm(byte[]) </seealso>
	   /// <seealso cref= #setClockAlarm(long,byte[]) </seealso>
	   /// <seealso cref= #setClockAlarmEnable(bool,byte[]) </seealso>
	   public virtual bool isClockAlarming(byte[] state)
	   {
		  return ((state [STATUS_REGISTER & 31] & TIMER_ALARM) != 0);
	   }

	   /// <summary>
	   /// Checks if the clock alarm is enabled.
	   /// </summary>
	   /// <param name="state"> current state of the device returned from <code>readDevice()</code>
	   /// </param>
	   /// <returns> true if clock alarm is enabled
	   /// </returns>
	   /// <seealso cref= com.dalsemi.onewire.container.OneWireSensor#readDevice() </seealso>
	   /// <seealso cref= #hasClockAlarm() </seealso>
	   /// <seealso cref= #isClockAlarming(byte[]) </seealso>
	   /// <seealso cref= #getClockAlarm(byte[]) </seealso>
	   /// <seealso cref= #setClockAlarm(long,byte[]) </seealso>
	   /// <seealso cref= #setClockAlarmEnable(bool,byte[]) </seealso>
	   public virtual bool isClockAlarmEnabled(byte[] state)
	   {
		  return ((state [CONTROL_REGISTER & 31] & TIMER_ALARM_SEARCH_FLAG) != 0);
	   }

	   /// <summary>
	   /// Checks if the device's oscillator is enabled.  The clock
	   /// will not increment if the clock oscillator is not enabled.
	   /// </summary>
	   /// <param name="state"> current state of the device returned from <code>readDevice()</code>
	   /// </param>
	   /// <returns> true if the clock is running
	   /// </returns>
	   /// <seealso cref= com.dalsemi.onewire.container.OneWireSensor#readDevice() </seealso>
	   /// <seealso cref= #canDisableClock() </seealso>
	   /// <seealso cref= #setClockRunEnable(bool,byte[]) </seealso>
	   public virtual bool isClockRunning(byte[] state)
	   {

		  //checks for equal to 0 since active low means clock is running
		  return ((state [CONTROL_REGISTER & 31] & OSCILLATOR_ENABLE_FLAG) == 0);
	   }

	   //--------
	   //-------- Clock 'set' Methods
	   //--------

	   /// <summary>
	   /// Sets the Real-Time clock.
	   /// The method <code>writeDevice()</code> must be called to finalize
	   /// changes to the device.  Note that multiple 'set' methods can
	   /// be called before one call to <code>writeDevice()</code>.
	   /// </summary>
	   /// <param name="time"> new value for the Real-Time clock, in milliseconds
	   /// since January 1, 1970 </param>
	   /// <param name="state"> current state of the device returned from <code>readDevice()</code>
	   /// </param>
	   /// <seealso cref= com.dalsemi.onewire.container.OneWireSensor#writeDevice(byte[]) </seealso>
	   /// <seealso cref= #getClock(byte[]) </seealso>
	   public virtual void setClock(long time, byte[] state)
	   {
		  DateTime d = new DateTime(TimeSpan.TicksPerMillisecond * time);

		  setTime(0x200, d.Hour, d.Minute, d.Second, false, state);
		  setDate(d.Year, d.Month, d.Day, state);

		  lock (this)
		  {
			 updatertc = true;
		  }
	   }

	   /// <summary>
	   /// Sets the clock alarm.
	   /// The method <code>writeDevice()</code> must be called to finalize
	   /// changes to the device.  Note that multiple 'set' methods can
	   /// be called before one call to <code>writeDevice()</code>.  Also note that
	   /// not all clock devices have alarms.  Check to see if this device has
	   /// alarms first by calling the <code>hasClockAlarm()</code> method.
	   /// </summary>
	   /// <param name="time"> - new value for the Real-Time clock alarm, in milliseconds
	   /// since January 1, 1970 </param>
	   /// <param name="state"> current state of the device returned from <code>readDevice()</code>
	   /// </param>
	   /// <exception cref="OneWireException"> if this device does not have clock alarms
	   /// </exception>
	   /// <seealso cref= com.dalsemi.onewire.container.OneWireSensor#writeDevice(byte[]) </seealso>
	   /// <seealso cref= #hasClockAlarm() </seealso>
	   /// <seealso cref= #isClockAlarmEnabled(byte[]) </seealso>
	   /// <seealso cref= #getClockAlarm(byte[]) </seealso>
	   /// <seealso cref= #isClockAlarming(byte[]) </seealso>
	   /// <seealso cref= #setClockAlarmEnable(bool,byte[]) </seealso>
	   public virtual void setClockAlarm(long time, byte[] state)
	   {

		  //can't do this because we need more info on the alarm
		  throw new OneWireException("Cannot set the DS1921 Clock Alarm through the Clock interface.");
	   }

	   /// <summary>
	   /// Enables or disables the oscillator, turning the clock 'on' and 'off'.
	   /// The method <code>writeDevice()</code> must be called to finalize
	   /// changes to the device.  Note that multiple 'set' methods can
	   /// be called before one call to <code>writeDevice()</code>.  Also note that
	   /// not all clock devices can disable their oscillators.  Check to see if this device can
	   /// disable its oscillator first by calling the <code>canDisableClock()</code> method.
	   /// </summary>
	   /// <param name="runEnable"> true to enable the clock oscillator </param>
	   /// <param name="state"> current state of the device returned from <code>readDevice()</code>
	   /// </param>
	   /// <seealso cref= com.dalsemi.onewire.container.OneWireSensor#writeDevice(byte[]) </seealso>
	   /// <seealso cref= #canDisableClock() </seealso>
	   /// <seealso cref= #isClockRunning(byte[]) </seealso>
	   public virtual void setClockRunEnable(bool runEnable, byte[] state)
	   {

		  // the oscillator enable is active low
		  setFlag(CONTROL_REGISTER, OSCILLATOR_ENABLE_FLAG, !runEnable, state);
	   }

	   /// <summary>
	   /// Enables or disables the clock alarm.
	   /// The method <code>writeDevice()</code> must be called to finalize
	   /// changes to the device.  Note that multiple 'set' methods can
	   /// be called before one call to <code>writeDevice()</code>.  Also note that
	   /// not all clock devices have alarms.  Check to see if this device has
	   /// alarms first by calling the <code>hasClockAlarm()</code> method.
	   /// </summary>
	   /// <param name="alarmEnable"> true to enable the clock alarm </param>
	   /// <param name="state"> current state of the device returned from <code>readDevice()</code>
	   /// </param>
	   /// <seealso cref= com.dalsemi.onewire.container.OneWireSensor#writeDevice(byte[]) </seealso>
	   /// <seealso cref= #hasClockAlarm() </seealso>
	   /// <seealso cref= #isClockAlarmEnabled(byte[]) </seealso>
	   /// <seealso cref= #getClockAlarm(byte[]) </seealso>
	   /// <seealso cref= #setClockAlarm(long,byte[]) </seealso>
	   /// <seealso cref= #isClockAlarming(byte[]) </seealso>
	   public virtual void setClockAlarmEnable(bool alarmEnable, byte[] state)
	   {
		  setFlag(CONTROL_REGISTER, TIMER_ALARM_SEARCH_FLAG, alarmEnable, state);
	   }
	}

}