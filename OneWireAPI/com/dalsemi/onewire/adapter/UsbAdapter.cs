﻿using System;
using System.Collections;
using System.Threading.Tasks;
using Windows.Devices.Usb;
using Windows.Devices.Enumeration;
using System.Diagnostics;

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

namespace com.dalsemi.onewire.adapter
{

    // imports
    using OneWireContainer = com.dalsemi.onewire.container.OneWireContainer;
    using com.dalsemi.onewire.utils;
    using System.Collections.Generic;
    using Windows.Storage.Streams;
    using System.Text;
    using System.IO;
    using Windows.Foundation;
    using System.Threading;/// <summary>
                           /// <para>This <code>DSPortAdapter</code> class was designed to be used for
                           /// the iB-IDE's emulator.  The <code>DumbAdapter</code> allows
                           /// programmers to add and remove <code>OneWireContainer</code>
                           /// objects that will be found in its search.  The Java iButton
                           /// emulator works by creating a class that subclasses all of
                           /// <code>OneWireContainer16</code>'s relevant methods and redirecting them
                           /// to the emulation code.  That object is then added to this class's
                           /// list of <code>OneWireContainer</code>s.</para>
                           /// 
                           /// <para>Note that methods such as <code>selectPort</code> and
                           /// <code>beginExclusive</code> by default do nothing.  This class is
                           /// mainly meant for debugging using an emulated iButton.  It will do
                           /// a poor job of debugging any multi-threading, port-sharing issues.
                           /// 
                           /// </para>
                           /// </summary>
                           /// <seealso cref= com.dalsemi.onewire.adapter.DSPortAdapter </seealso>
                           /// <seealso cref= com.dalsemi.onewire.container.OneWireContainer
                           /// 
                           /// @version    0.00, 16 Mar 2001
                           /// @author     K </seealso>
    public class UsbAdapter : DSPortAdapter
    {
        //--------
        //-------- Variables
        //--------

        /// <summary>
        /// Enable/disable debug messages </summary>
        private static bool doDebugMessages = true;
        /// <summary>
        /// The USB port name of this object</summary>
        private readonly string usbDeviceName;
        /// <summary>
        /// The USB port object for setting USB port parameters </summary>
        private UsbDevice usbDevice = null;
        /// <summary>
        /// The DeviceInformation device id used to open the USB port</summary>
        private string deviceId;

        /// <summary>
        /// USB Adapter packet builder </summary>
        internal UsbPacketBuilder uBuild;

        /// <summary>
        /// State of the OneWire </summary>
        private OneWireState owState;

        /// <summary>
        /// USB Adapter state </summary>
        private UsbAdapterState uState;

        /// <summary>
        /// Flag to indicate have a local begin/end Exclusive use of serial </summary>
        private bool haveLocalUse;
        private object syncObject;

        /// <summary>
        /// String name of the current opened port </summary>
        private bool adapterPresent;

        /// <summary>
        /// Flag to indicate more than expected byte received in a transaction </summary>
        private bool extraBytesReceived;

        /// <summary>
        /// Adapter Status registers </summary>
        private Ds2490.UsbStatusPacket statusPacket;

        /// <summary>
        /// Event used to read interrupt only once </summary>
        private AutoResetEvent waitStatus;

        /// <summary>
        /// Vector of thread hash codes that have done an open but no close </summary>
        private readonly ArrayList users = new ArrayList(4);

        internal int containers_index = 0;

        private ArrayList containers = new ArrayList();


        public UsbAdapter()
        {
            usbDevice = null;
            owState = new OneWireState();
            uState = new UsbAdapterState(owState);
            uBuild = new UsbPacketBuilder(uState);
            statusPacket = new Ds2490.UsbStatusPacket();
            waitStatus = new AutoResetEvent(false);
            adapterPresent = false;
            haveLocalUse = false;
            syncObject = new object();
        }

        /// <summary>
        /// Adds a <code>OneWireContainer</code> to the list of containers that
        /// this adapter object will find.
        /// </summary>
        /// <param name="c"> represents a 1-Wire device that this adapter will report from a search </param>
        public virtual void addContainer(OneWireContainer c)
        {
            lock (containers)
            {
                containers.Add(c);
            }
        }

        /// <summary>
        /// Removes a <code>OneWireContainer</code> from the list of containers that
        /// this adapter object will find.
        /// </summary>
        /// <param name="c"> represents a 1-Wire device that this adapter should no longer
        ///        report as found by a search </param>
        public virtual void removeContainer(OneWireContainer c)
        {
            lock (containers)
            {
                containers.Remove(c);
            }
        }


        /// <summary>
        /// Hashtable to contain the user replaced OneWireContainers
        /// </summary>
        private Hashtable registeredOneWireContainerClasses = new Hashtable(5);

        /// <summary>
        /// Byte array of families to include in search
        /// </summary>
        private byte[] include;

        /// <summary>
        /// Byte array of families to exclude from search
        /// </summary>
        private byte[] exclude;

        //--------
        //-------- Methods
        //--------

        /// <summary>
        /// Retrieves the name of the port adapter as a string.  The 'Adapter'
        /// is a device that connects to a 'port' that allows one to
        /// communicate with an iButton or other 1-Wire device.  As example
        /// of this is 'DS9097U'.
        /// </summary>
        /// <returns>  <code>String</code> representation of the port adapter. </returns>
        public override string AdapterName
        {
            get
            {
                return "DS2490";
            }
        }

        /// <summary>
        /// Retrieves a description of the port required by this port adapter.
        /// An example of a 'Port' would 'serial communication port'.
        /// </summary>
        /// <returns>  <code>String</code> description of the port type required. </returns>
        public override string PortTypeDescription
        {
            get
            {
                return "DS2490 USB Port";
            }
        }

        /// <summary>
        /// Retrieves a version string for this class.
        /// </summary>
        /// <returns>  version string </returns>
        public override string ClassVersion
        {
            get
            {
                return "1.00";
            }
        }

        //--------
        //-------- Port Selection
        //--------

        /// <summary>
        /// Retrieves a list of the platform appropriate port names for this
        /// adapter.  A port must be selected with the method 'selectPort'
        /// before any other communication methods can be used.  Using
        /// a communcation method before 'selectPort' will result in
        /// a <code>OneWireException</code> exception.
        /// </summary>
        /// <returns>  <code>Enumeration</code> of type <code>String</code> that contains the port
        /// names </returns>
        public override System.Collections.IEnumerator PortNames
        {
            get
            {
                var t = Task<IEnumerator>.Run(async () =>
                {
                    string aqs = UsbDevice.GetDeviceSelector(Ds2490.DeviceVid, Ds2490.DevicePid);
                    var myDevices = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(aqs, null);
                    DeviceInformationCollection DeviceList = myDevices;
                    //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
                    if (doDebugMessages)
                    {
                        foreach (var item in DeviceList)
                            Debug.WriteLine("\t" + item.Id);
                    }
                    //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
                    return ((IEnumerable<DeviceInformation>)DeviceList).GetEnumerator();
                });

                t.Wait();

                if (t.Status == TaskStatus.RanToCompletion)
                {
                    return t.Result;
                }
                else
                {
                    throw new System.Exception("Unable to retrieve list of USB devices");
                }
            }
        }

        /// <summary>
        /// This method does nothing in <code>DumbAdapter</code>.
        /// 
        /// </summary>
        public override void registerOneWireContainerClass(int family, Type OneWireContainerClass)
        {
        }

        /// <summary>
        /// This method does nothing in <code>DumbAdapter</code>.
        /// </summary>
        /// <param name="portName">  name of the target port, retrieved from
        /// getPortNames()
        /// </param>
        /// <returns> always returns <code>true</code> </returns>
        public override bool selectPort(string portName)
        {
            //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
            if (doDebugMessages)
            {
                Debug.WriteLine("USBAdapter.selectPort() called");
            }
            //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
            if (doDebugMessages)
            {
                Debug.WriteLine("USBAdapter.selectPort: System.Enivronment.CurrentManagedThreadId()=" + Environment.CurrentManagedThreadId);
            }
            //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
            // record this thread as an owner
            if (users.IndexOf(Environment.CurrentManagedThreadId) == -1)
            {
                users.Add(Environment.CurrentManagedThreadId);
            }

            if (PortOpen)
                return true;

            try
            {
                var t = Task<UsbDevice>.Run(async () =>
                {
                    // @"USB\VID_04FA&PID_2490\6&F0F8E95&0&2"
                    string[] st = portName.Split(new char[] { '\\' });
                    StringBuilder deviceInstance = new StringBuilder();
                    deviceInstance.Append(@"\\?\");
                    deviceInstance.Append(st[0]);
                    deviceInstance.Append('#');
                    deviceInstance.Append(st[1]);
                    deviceInstance.Append('#');
                    deviceInstance.Append(st[2].ToLower());
                    deviceInstance.Append('#');
                    deviceInstance.Append("{dee824ef-729b-4a0e-9c14-b7117d33a817}");

                    deviceId = deviceInstance.ToString();
                    return await UsbDevice.FromIdAsync(deviceId);
                });

                t.Wait();
                if (t.Status != TaskStatus.RanToCompletion)
                {
                    throw new System.IO.IOException("Failed to open (" + portName + ")");
                }

                usbDevice = t.Result;

                //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
                if (doDebugMessages)
                {
                    Debug.WriteLine("SerialService.openPort: Port Openend (" + portName + ")");
                }
                //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
            }
            catch (Exception e)
            {
                // close the port if we have an object
                freePort();

                throw new System.IO.IOException("Failed to open (" + portName + ") :" + e);
            }

            return true;
        }

        /// <summary>
        /// This method does nothing in <code>DumbAdapter</code>.
        /// </summary>
        public override void freePort()
        {
            if (usbDevice != null)
            {
                usbDevice.Dispose();
                usbDevice = null;
            }
        }

        /// <summary>
        /// Retrieves the name of the selected port as a <code>String</code>.
        /// </summary>
        /// <returns>  the deviceID of port </returns>
        public override string PortName
        {
            get
            {
                return deviceId;
            }
        }

        /// <summary>
        /// Retrieves the open state of the selected port as a <code>bool</code>.
        /// </summary>
        /// <returns>  bool indicating port open state </returns>
        public virtual bool PortOpen
        {
            get
            {
                lock (this)
                {
                    return (usbDevice != null) ? true : false;
                }
            }
        }

        //--------
        //-------- Adapter detection
        //--------

        private bool SendCommand(byte req, uint value, uint index, string description)
        {
            bool result = true;

            if (doDebugMessages)
            {
                Debug.WriteLine("DEBUG: SetupCommand - " + description);
            }

            // try to aquire the port
            try
            {
                if (!PortOpen)
                {
                    throw new System.IO.IOException("Port Not Open");
                }

                //
                // RESET_DEVICE
                //
                var t = Task<uint>.Run(async () =>
                {
                    UsbSetupPacket setupPacket = new UsbSetupPacket
                    {
                        RequestType = new UsbControlRequestType
                        {
                            Direction = UsbTransferDirection.Out,
                            Recipient = UsbControlRecipient.Device,
                            ControlTransferType = UsbControlTransferType.Vendor
                        },
                        Request = req,
                        Value = value,
                        Index = index,
                        Length = 0
                    };

                    return await usbDevice.SendControlOutTransferAsync(setupPacket);
                });

                t.Wait();

                if (t.Status != TaskStatus.RanToCompletion)
                {
                    Debug.WriteLine("USBAdapter-" + description + " failed to send packet");
                }
                System.Diagnostics.Debug.WriteLine("SendControlOut HResult = {0:X08}", t.Result.ToString());
            }
            catch (System.IO.IOException e)
            {
                if (doDebugMessages)
                {
                    Debug.WriteLine("USBAdapter-" + description + ": " + e);
                }
            }

            return result;
        }

        private UInt32 registeredInterruptPipeIndex; // Pipe index of the pipe we that we registered for. Only valid if registeredInterrupt is true
        private bool registeredInterrupt;

        private void OnStatusChangeEvent(UsbInterruptInPipe sender, UsbInterruptInEventArgs eventArgs)
        {
            IBuffer buffer = eventArgs.InterruptData;

            if (buffer.Length > 0)
            {
                DataReader reader = DataReader.FromBuffer(buffer);

                statusPacket.UpdateUsbStatusPacket(reader, buffer.Length);

                waitStatus.Set();
            }
        }

        private TypedEventHandler<UsbInterruptInPipe, UsbInterruptInEventArgs> interruptEventHandler = null;

        private void GetStatus(out byte nResultRegisters)
        {
            interruptEventHandler = new TypedEventHandler<UsbInterruptInPipe, UsbInterruptInEventArgs>(this.OnStatusChangeEvent);

            statusPacket.Updated = false;
            statusPacket.CommResultCodes = null;

            RegisterForInterruptEvent(Ds2490.Pipe.InterruptInPipeIndex, interruptEventHandler);

            while (!statusPacket.Updated)
                waitStatus.WaitOne();

            UnregisterFromInterruptEvent();

            if (statusPacket.CommResultCodes != null)
                nResultRegisters = (byte)statusPacket.CommResultCodes.Length;
            else
                nResultRegisters = 0;
        }

        /// <summary>
        /// Register for the interrupt that is triggered when the device sends an interrupt to us
        /// 
        /// The DefaultInterface on the the device is the first interface on the device. We navigate to
        /// the InterruptInPipes because that collection contains all the interrupt in pipes for the
        /// selected interface setting.
        ///
        /// Each pipe has a property that links to an EndpointDescriptor. This descriptor can be used to find information about
        /// the pipe (e.g. type, id, etc...). The EndpointDescriptor trys to mirror the EndpointDescriptor that is defined in the Usb Spec.
        ///
        /// The function also saves the event token so that we can unregister from the even later on.
        /// </summary>
        /// <param name="pipeIndex">The index of the pipe found in UsbInterface.InterruptInPipes. It is not the endpoint number</param>
        /// <param name="eventHandler">Event handler that will be called when the event is raised</param>
        private void RegisterForInterruptEvent(UInt32 pipeIndex, TypedEventHandler<UsbInterruptInPipe, UsbInterruptInEventArgs> eventHandler)
        {
            var interruptInPipes = usbDevice.DefaultInterface.InterruptInPipes;

            if (!registeredInterrupt && (pipeIndex < interruptInPipes.Count))
            {
                var interruptInPipe = interruptInPipes[(int)pipeIndex];

                registeredInterrupt = true;
                registeredInterruptPipeIndex = pipeIndex;

                // Save the interrupt handler so we can use it to unregister
                interruptEventHandler = eventHandler;

                interruptInPipe.DataReceived += interruptEventHandler;
            }
        }

        /// <summary>
        /// Unregisters from the interrupt event that was registered for in the RegisterForInterruptEvent();
        /// </summary>
        private void UnregisterFromInterruptEvent()
        {
            if (registeredInterrupt)
            {
                // Search for the correct pipe that we know we used to register events
                var interruptInPipe = usbDevice.DefaultInterface.InterruptInPipes[(int)registeredInterruptPipeIndex];
                interruptInPipe.DataReceived -= interruptEventHandler;

                registeredInterrupt = false;
            }
        }

        /// <summary>
        /// Check to see if there is a short on the 1-Wire bus. Used to stop 
        /// communication with the DS2490 while the short is in effect to not
        /// overrun the buffers.
        /// </summary>
        /// <param name="present">flag set (1) if device presence detected</param>
        /// <param name="vpp">flag set (1) if Vpp programming voltage detected</param>
        /// <returns>
        /// true  - DS2490 1-Wire is NOT shorted
        /// true - Could not detect DS2490 or 1-Wire shorted
        /// </returns>
        private bool ShortCheck(out bool present, out bool vpp)
        {
            byte nResultRegisters;

            present = false;

            GetStatus(out nResultRegisters);

            // get vpp present flag
            vpp = ((statusPacket.StatusFlags & Ds2490.STATUSFLAGS_12VP) != 0);

            //	Check for short
            if (statusPacket.CommBufferStatus != 0)
            {
                return false;
            }
            else
            {
                // check for short
                for (var i = 0; i < nResultRegisters; i++)
                {
                    // check for SH bit (0x02), ignore 0xA5
                    if ((statusPacket.CommResultCodes[i] & Ds2490.COMMCMDERRORRESULT_SH) != 0)
                    {
                        // short detected
                        return false;
                    }
                }
            }

            // check for No 1-Wire device condition
            present = true;
            // loop through result registers
            for (var i = 0; i < nResultRegisters; i++)
            {
                // only check for error conditions when the condition is not a ONEWIREDEVICEDETECT
                if (statusPacket.CommResultCodes[i] != Ds2490.ONEWIREDEVICEDETECT)
                {
                    // check for NRS bit (0x01)
                    if ((statusPacket.CommResultCodes[i] & Ds2490.COMMCMDERRORRESULT_NRS) != 0)
                    {
                        // empty bus detected
                        present = false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        ///  Read and verify the baud rate with the DS2480 chip and perform a
        ///  single bit MicroLAN operation.  This is used as a DS2480 detect.
        /// </summary>
        ///  <returns>  'true' if the correct baud rate and bit operation
        ///           was read from the DS2480
        /// </returns>
        ///  <exception cref="OneWireIOException"> on a 1-Wire communication error </exception>
        private bool uVerify()
        {
            try
            {
                bool present = false;
                bool vpp = false;

                return ShortCheck(out present, out vpp);
            }
            catch (IOException ioe)
            {
                if (doDebugMessages)
                {
                    Debug.WriteLine("USBAdapter-uVerify: " + ioe);
                }
            }
            catch (OneWireIOException e)
            {
                if (doDebugMessages)
                {
                    Debug.WriteLine("USBAdapter-uVerify: " + e);
                }
            }

            return false;
        }

        /// <summary>
        /// Write the raw U packet and then read the result.
        /// </summary>
        /// <param name="tempBuild">  the U Packet Build where the packet to send
        ///                     resides
        /// </param>
        /// <returns>  the result array
        /// </returns>
        /// <exception cref="OneWireIOException"> on a 1-Wire communication error </exception>
        private byte[] uTransaction(UPacketBuilder tempBuild)
        {
            int offset;
            byte[] ret_buffer = null;

            Debugger.Break();

            try
            {
                using (MemoryStream inBuffer = new MemoryStream())
                {
                    // loop to send all of the packets
                    for (System.Collections.IEnumerator packet_enum = tempBuild.Packets; packet_enum.MoveNext();)
                    {

                        // get the next packet
                        RawSendPacket pkt = (RawSendPacket)packet_enum.Current;

                        // bogus packet to indicate need to wait for long DS2480 alarm reset
                        if ((pkt.buffer.Length == 0) && (pkt.returnLength == 0))
                        {
                            Thread.Sleep(6);

                            continue;
                        }

                        // remember number of bytes in input
                        offset = (int)inBuffer.Length;

                        // send the packet
                        pkt.writer.Flush();
                        //TODO serial.write(pkt.buffer.ToArray());

                        // wait on returnLength bytes in inBound
                        //TODO byte[] read = serial.readWithTimeout(pkt.returnLength);
                        //TODO inBuffer.Write(read, 0, read.Length);
                    }

                    // read the return packet
                    ret_buffer = new byte[inBuffer.Length];

                    ret_buffer = inBuffer.ToArray();

                    // check for extra bytes in inBuffer
                    extraBytesReceived = (inBuffer.Length > tempBuild.totalReturnLength);
                }

                return ret_buffer;
            }
            catch (IOException e)
            {

                // need to check on adapter
                adapterPresent = false;

                // pass it on
                throw new OneWireIOException(e.ToString());
            }
        }

        /// <summary>
        /// Verify that the DS2480 based adapter is present on the open port.
        /// </summary>
        /// <returns> 'true' if adapter present
        /// </returns>
        /// <exception cref="OneWireException"> - if port not selected </exception>
        private bool uAdapterPresent()
        {
            bool rt = true;

            // check if adapter has already be verified to be present
            if (!adapterPresent)
            {

                // do a master reset
                uMasterReset();

                // attempt to verify
                if (!uVerify())
                {

                    // do a master reset and try again
                    uMasterReset();

                    if (!uVerify())
                    {

                        rt = false;
                    }
                }
            }

            adapterPresent = rt;

            if (doDebugMessages)
            {
                Debug.WriteLine("DEBUG: AdapterPresent result: " + rt);
            }

            return rt;
        }

        /// <summary>
        /// Do a master reset on the DS2490.  This initiates
        /// a master rest cycle.
        /// </summary>
        private void uMasterReset()
        {

            if (doDebugMessages)
            {
                Debug.WriteLine("DEBUG: uMasterReset");
            }

            SendCommand(
                Ds2490.CMD_TYPE.CONTROL,
                Ds2490.CTL.RESET_DEVICE,
                0x0000,
                "uMasterReset"
                );

            // reset state variables to match device
            owState.oneWireSpeed = SPEED_REGULAR;
            uState.uSpeedMode = UsbAdapterState.USPEED_REGULAR;

            // set the strong pullup duration to infinite
            SendCommand(
                Ds2490.CMD_TYPE.COMM,
                Ds2490.COMM.SET_DURATION | Ds2490.COMM.IM,
                0x0000,
                "Set duration to infinite");

            // set the 12V pullup duration to 512us
            SendCommand(
                Ds2490.CMD_TYPE.COMM,
                Ds2490.COMM.SET_DURATION | Ds2490.COMM.IM | Ds2490.COMM.TYPE,
                0x0040,
                "Set 12V pullup duration to 512us");

            // disable strong pullup, but leave program pulse enabled (faster)
            SendCommand(
                Ds2490.CMD_TYPE.MODE,
                Ds2490.Mode.PULSE_EN,
                Ds2490.ENABLEPULSE_PRGE,
                "disable strong pullup, but leave program pulse enabled");

        }

        /// <summary>
        /// Detect adapter presence on the selected port.
        /// </summary>
        /// <returns>  <code>true</code> if the adapter is confirmed to be connected to
        /// the selected port, <code>false</code> if the adapter is not connected.
        /// </returns>
        /// <exception cref="OneWireIOException"> </exception>
        /// <exception cref="OneWireException"> </exception>
        public override bool adapterDetected()
        {
            bool rt;

            try
            {

                // acquire exclusive use of the port
                beginLocalExclusive();
                uAdapterPresent();

                rt = uVerify();
            }
            catch (OneWireException)
            {
                rt = false;
            }
            finally
            {

                // release local exclusive use of port
                endLocalExclusive();
            }

            return rt;
        }

        //--------
        //-------- Adapter features
        //--------

        /// <summary>
        /// Applications might check this method and not attempt operation unless this method
        /// returns <code>true</code>. To make sure that a wide variety of applications can use this class,
        /// this method always returns <code>true</code>.
        /// </summary>
        /// <returns>  <code>true</code>
        ///  </returns>
        public override bool canOverdrive()
        {
            return true;
        }

        /// <summary>
        /// Applications might check this method and not attempt operation unless this method
        /// returns <code>true</code>. To make sure that a wide variety of applications can use this class,
        /// this method always returns <code>true</code>.
        /// </summary>
        /// <returns>  <code>true</code> </returns>
        public override bool canHyperdrive()
        {
            return true;
        }

        /// <summary>
        /// Applications might check this method and not attempt operation unless this method
        /// returns <code>true</code>. To make sure that a wide variety of applications can use this class,
        /// this method always returns <code>true</code>.
        /// </summary>
        /// <returns>  <code>true</code> </returns>
        public override bool canFlex()
        {
            return true;
        }

        /// <summary>
        /// Applications might check this method and not attempt operation unless this method
        /// returns <code>true</code>. To make sure that a wide variety of applications can use this class,
        /// this method always returns <code>true</code>.
        /// </summary>
        /// <returns>  <code>true</code> </returns>
        public override bool canProgram()
        {
            return false;
        }

        /// <summary>
        /// Applications might check this method and not attempt operation unless this method
        /// returns <code>true</code>. To make sure that a wide variety of applications can use this class,
        /// this method always returns <code>true</code>.
        /// </summary>
        /// <returns>  <code>true</code> </returns>
        public override bool canDeliverPower()
        {
            return true;
        }

        /// <summary>
        /// Applications might check this method and not attempt operation unless this method
        /// returns <code>true</code>. To make sure that a wide variety of applications can use this class,
        /// this method always returns <code>true</code>.
        /// </summary>
        /// <returns>  <code>true</code> </returns>
        public override bool canDeliverSmartPower()
        {
            return true;
        }

        /// <summary>
        /// Applications might check this method and not attempt operation unless this method
        /// returns <code>true</code>. To make sure that a wide variety of applications can use this class,
        /// this method always returns <code>true</code>.
        /// </summary>
        /// <returns>  <code>true</code> </returns>
        public override bool canBreak()
        {
            return true;
        }

        //--------
        //-------- Finding iButtons and 1-Wire devices
        //--------

        /// <summary>
        /// Returns <code>true</code> if the first iButton or 1-Wire device
        /// is found on the 1-Wire Network.
        /// If no devices are found, then <code>false</code> will be returned.
        /// </summary>
        /// <returns>  <code>true</code> if an iButton or 1-Wire device is found. </returns>
        public override bool findFirstDevice()
        {

            // reset the current search
            owState.searchLastDiscrepancy = 0;
            owState.searchFamilyLastDiscrepancy = 0;
            owState.searchLastDevice = false;

            // search for the first device using next
            return findNextDevice();
        }

        /// <summary>
        /// Returns <code>true</code> if the next iButton or 1-Wire device
        /// is found. The previous 1-Wire device found is used
        /// as a starting point in the search.  If no more devices are found
        /// then <code>false</code> will be returned.
        /// </summary>
        /// <returns>  <code>true</code> if an iButton or 1-Wire device is found. </returns>
        public override bool findNextDevice()
        {
            bool search_result;

            try
            {

                // acquire exclusive use of the port
                beginLocalExclusive();

                // check for previous last device
                if (owState.searchLastDevice)
                {
                    owState.searchLastDiscrepancy = 0;
                    owState.searchFamilyLastDiscrepancy = 0;
                    owState.searchLastDevice = false;

                    return false;
                }

                // check for 'first' and only 1 target
                if ((owState.searchLastDiscrepancy == 0) && (owState.searchLastDevice == false) && (owState.searchIncludeFamilies.Length == 1))
                {

                    // set the search to find the 1 target first
                    owState.searchLastDiscrepancy = 64;

                    // create an id to set
                    byte[] new_id = new byte[8];

                    // set the family code
                    new_id[0] = owState.searchIncludeFamilies[0];

                    // clear the rest
                    for (int i = 1; i < 8; i++)
                    {
                        new_id[i] = 0;
                    }

                    // set this new ID
                    Array.Copy(new_id, 0, owState.ID, 0, 8);
                }

                // loop until the correct type is found or no more devices
                do
                {

                    // perform a search and keep the result
                    search_result = search(owState);

                    if (search_result)
                    {

                        // check if not in exclude list
                        bool is_excluded = false;

                        for (int i = 0; i < owState.searchExcludeFamilies.Length; i++)
                        {
                            if (owState.ID[0] == owState.searchExcludeFamilies[i])
                            {
                                is_excluded = true;

                                break;
                            }
                        }

                        // if not in exclude list then check for include list
                        if (!is_excluded)
                        {

                            // loop through the include list
                            bool is_included = false;

                            for (int i = 0; i < owState.searchIncludeFamilies.Length; i++)
                            {
                                if (owState.ID[0] == owState.searchIncludeFamilies[i])
                                {
                                    is_included = true;

                                    break;
                                }
                            }

                            // check if include list or there is no include list
                            if (is_included || (owState.searchIncludeFamilies.Length == 0))
                            {
                                return true;
                            }
                        }
                    }

                    // skip the current type if not last device
                    if (!owState.searchLastDevice && (owState.searchFamilyLastDiscrepancy != 0))
                    {
                        owState.searchLastDiscrepancy = owState.searchFamilyLastDiscrepancy;
                        owState.searchFamilyLastDiscrepancy = 0;
                        owState.searchLastDevice = false;
                    }

                    // end of search so reset and return
                    else
                    {
                        owState.searchLastDiscrepancy = 0;
                        owState.searchFamilyLastDiscrepancy = 0;
                        owState.searchLastDevice = false;
                        search_result = false;
                    }
                } while (search_result);

                // device not found
                return false;
            }
            finally
            {

                // release local exclusive use of port
                endLocalExclusive();
            }
        }


        /// <summary>
        /// Copies the 'current' 1-Wire device address being used by the adapter into
        /// the array.  This address is the last iButton or 1-Wire device found
        /// in a search (findNextDevice()...).
        /// This method copies into a user generated array to allow the
        /// reuse of the buffer.  When searching many iButtons on the one
        /// wire network, this will reduce the memory burn rate.
        /// </summary>
        /// <param name="address"> An array to be filled with the current iButton address. </param>
        /// <seealso cref=   com.dalsemi.onewire.utils.Address </seealso>
        public override void getAddress(byte[] address)
        {
            Array.Copy(owState.ID, 0, address, 0, 8);
        }

        /// <summary>
        /// Copies the provided 1-Wire device address into the 'current'
        /// array.  This address will then be used in the getDeviceContainer()
        /// method.  Permits the adapter instance to create containers
        /// of devices it did not find in a search.
        /// </summary>
        /// <param name="address"> An array to be copied into the current iButton
        ///         address. </param>
        public virtual byte[] Address
        {
            set
            {
                Array.Copy(value, 0, owState.ID, 0, 8);
            }
        }

        ///// <summary>
        ///// Gets the 'current' 1-Wire device address being used by the adapter as a long.
        ///// This address is the last iButton or 1-Wire device found
        ///// in a search (findNextDevice()...).
        ///// </summary>
        ///// <returns> <code>long</code> representation of the iButton address </returns>
        ///// <seealso cref=   com.dalsemi.onewire.utils.Address </seealso>
        //public override long AddressAsLong
        //{
        // get
        // {
        // byte[] address = new byte [8];

        // getAddress(address);

        // return Address.toLong(address);
        // }
        //}

        ///// <summary>
        ///// Gets the 'current' 1-Wire device address being used by the adapter as a String.
        ///// This address is the last iButton or 1-Wire device found
        ///// in a search (findNextDevice()...).
        ///// </summary>
        ///// <returns> <code>String</code> representation of the iButton address </returns>
        ///// <seealso cref=   com.dalsemi.onewire.utils.Address </seealso>
        //public override string AddressAsString
        //{
        // get
        // {
        // byte[] address = new byte [8];

        // getAddress(address);

        // return Address.ToString(address);
        // }
        //}

        /// <summary>
        /// Verifies that the iButton or 1-Wire device specified is present on
        /// the 1-Wire Network. This does not affect the 'current' device
        /// state information used in searches (findNextDevice...).
        /// </summary>
        /// <param name="address">  device address to verify is present
        /// </param>
        /// <returns>  <code>true</code> if device is present, else
        ///         <code>false</code>.
        /// </returns>
        /// <seealso cref=   com.dalsemi.onewire.utils.Address </seealso>
        public override bool isPresent(byte[] address)
        {
            try
            {

                // acquire exclusive use of the port
                beginLocalExclusive();

                // make sure adapter is present
                if (adapterDetected())
                {

                    // check for pending power conditions
                    if (owState.oneWireLevel != LEVEL_NORMAL)
                    {
                        setPowerNormal();
                    }

                    // if in overdrive, then use the block method in super
                    if (owState.oneWireSpeed == SPEED_OVERDRIVE)
                    {
                        return blockIsPresent(address, false);
                    }

                    // create a private OneWireState
                    OneWireState onewire_state = new OneWireState();

                    // set the ID to the ID of the iButton passes to this method
                    Array.Copy(address, 0, onewire_state.ID, 0, 8);

                    // set the state to find the specified device
                    onewire_state.searchLastDiscrepancy = 64;
                    onewire_state.searchFamilyLastDiscrepancy = 0;
                    onewire_state.searchLastDevice = false;
                    onewire_state.searchOnlyAlarmingButtons = false;

                    // perform a search
                    if (search(onewire_state))
                    {

                        // compare the found device with the desired device
                        for (int i = 0; i < 8; i++)
                        {
                            if (address[i] != onewire_state.ID[i])
                            {
                                return false;
                            }
                        }

                        // must be the correct device
                        return true;
                    }

                    // failed to find device
                    return false;
                }
                else
                {
                    throw new OneWireIOException("Error communicating with adapter");
                }
            }
            finally
            {

                // release local exclusive use of port
                endLocalExclusive();
            }
        }
    
	   /// <summary>
	   /// Verifies that the iButton or 1-Wire device specified is present on
	   /// the 1-Wire Network. This does not affect the 'current' device
	   /// state information used in searches (findNextDevice...).
	   /// </summary>
	   /// <param name="address">  device address to verify is present
	   /// </param>
	   /// <returns>  <code>true</code> if device is present, else
	   ///         <code>false</code>.
	   /// </returns>
	   /// <seealso cref=   com.dalsemi.onewire.utils.Address </seealso>
	   public override bool isPresent(long address)
	   {
			lock (containers)
			{
				for (int i = 0;i < containers.Count;i++)
				{
					OneWireContainer temp = (OneWireContainer) containers[i];
					long addr = temp.AddressAsLong;
					if (addr == address)
					{
						return true;
					}
				}
			}
			return false;
	   }

        /// <summary>
        /// Verifies that the iButton or 1-Wire device specified is present
        /// on the 1-Wire Network and in an alarm state. This does not
        /// affect the 'current' device state information used in searches
        /// (findNextDevice...).
        /// </summary>
        /// <param name="address">  device address to verify is present and alarming
        /// </param>
        /// <returns>  <code>true</code> if device is present and alarming else
        /// <code>false</code>.
        /// </returns>
        /// <exception cref="OneWireIOException"> on a 1-Wire communication error </exception>
        /// <exception cref="OneWireException"> on a setup error with the 1-Wire adapter
        /// </exception>
        /// <seealso cref=    com.dalsemi.onewire.utils.Address </seealso>
        public override bool isAlarming(byte[] address)
        {
            try
            {

                // acquire exclusive use of the port
                beginLocalExclusive();

                // make sure adapter is present
                if (uAdapterPresent())
                {

                    // check for pending power conditions
                    if (owState.oneWireLevel != LEVEL_NORMAL)
                    {
                        setPowerNormal();
                    }

                    // if in overdrive, then use the block method in super
                    if (owState.oneWireSpeed == SPEED_OVERDRIVE)
                    {
                        return blockIsPresent(address, true);
                    }

                    // create a private OneWireState
                    OneWireState onewire_state = new OneWireState();

                    // set the ID to the ID of the iButton passes to this method
                    Array.Copy(address, 0, onewire_state.ID, 0, 8);

                    // set the state to find the specified device (alarming)
                    onewire_state.searchLastDiscrepancy = 64;
                    onewire_state.searchFamilyLastDiscrepancy = 0;
                    onewire_state.searchLastDevice = false;
                    onewire_state.searchOnlyAlarmingButtons = true;

                    // perform a search
                    if (search(onewire_state))
                    {

                        // compare the found device with the desired device
                        for (int i = 0; i < 8; i++)
                        {
                            if (address[i] != onewire_state.ID[i])
                            {
                                return false;
                            }
                        }

                        // must be the correct device
                        return true;
                    }

                    // failed to find any alarming device
                    return false;
                }
                else
                {
                    throw new OneWireIOException("Error communicating with adapter");
                }
            }
            finally
            {

                // release local exclusive use of port
                endLocalExclusive();
            }
        }

	   /// <summary>
	   /// Selects the specified iButton or 1-Wire device by broadcasting its
	   /// address.  With a <code>DumbAdapter</code>, this method simply
	   /// returns true.
	   /// 
	   /// Warning, this does not verify that the device is currently present
	   /// on the 1-Wire Network (See isPresent).
	   /// </summary>
	   /// <param name="address">    address of iButton or 1-Wire device to select
	   /// </param>
	   /// <returns>  <code>true</code> if device address was sent, <code>false</code>
	   /// otherwise.
	   /// </returns>
	   /// <seealso cref= #isPresent(byte[]) </seealso>
	   /// <seealso cref=   com.dalsemi.onewire.utils.Address </seealso>
	   public override bool select(byte[] address)
	   {
			return isPresent(address);
	   }

        /// <summary>
        /// Selects the specified iButton or 1-Wire device by broadcasting its
        /// address.  With a <code>DumbAdapter</code>, this method simply
        /// returns true.
        /// 
        /// Warning, this does not verify that the device is currently present
        /// on the 1-Wire Network (See isPresent).
        /// </summary>
        /// <param name="address">    address of iButton or 1-Wire device to select
        /// </param>
        /// <returns>  <code>true</code> if device address was sent, <code>false</code>
        /// otherwise.
        /// </returns>
        /// <seealso cref= #isPresent(byte[]) </seealso>
        /// <seealso cref=   com.dalsemi.onewire.utils.Address </seealso>
        //public override bool select(long address)
        //{
        //return select(Address.toByteArray(address));
        //}

        /// <summary>
        /// Selects the specified iButton or 1-Wire device by broadcasting its
        /// address.  With a <code>DumbAdapter</code>, this method simply
        /// returns true.
        /// 
        /// Warning, this does not verify that the device is currently present
        /// on the 1-Wire Network (See isPresent).
        /// </summary>
        /// <param name="address">    address of iButton or 1-Wire device to select
        /// </param>
        /// <returns>  <code>true</code> if device address was sent, <code>false</code>
        /// otherwise.
        /// </returns>
        /// <seealso cref=    #isPresent(byte[]) </seealso>
        /// <seealso cref=   com.dalsemi.onewire.utils.Address </seealso>
        //public override bool select(string address)
        //{
        //return select(Address.toByteArray(address));
        //}

        //--------
        //-------- Support methods
        //--------

        /// <summary>
        /// Peform a search using the oneWire state provided
        /// </summary>
        ///  <param name="mState">  current OneWire state used to do the search
        /// </param>
        /// <exception cref="OneWireIOException"> on a 1-Wire communication error </exception>
        /// <exception cref="OneWireException"> on a setup error with the 1-Wire adapter </exception>
        private bool search(OneWireState mState)
        {
            int reset_offset = 0;

            Debugger.Break();

            // make sure adapter is present
            if (uAdapterPresent())
            {

                // check for pending power conditions
                if (owState.oneWireLevel != LEVEL_NORMAL)
                {
                    setPowerNormal();
                }

                // set the correct baud rate to stream this operation
//TODO                StreamingSpeed = UPacketBuilder.OPERATION_SEARCH;

                //// reset the packet
                //uBuild.restart();

                //// add a reset/ search command
                //if (!mState.skipResetOnSearch)
                //{
                //    reset_offset = oneWireReset();
                //}

                //if (mState.searchOnlyAlarmingButtons)
                //{
                //    uBuild.dataByte(ALARM_SEARCH_CMD);
                //}
                //else
                //{
                //    uBuild.dataByte(NORMAL_SEARCH_CMD);
                //}

                //// add search sequence based on mState
                //int search_offset = uBuild.search(mState);

                //// send/receive the search
                //byte[] result_array = uTransaction(uBuild);

                //// interpret search result and return
                //if (!mState.skipResetOnSearch)
                //{
                //    uBuild.interpretOneWireReset(result_array[reset_offset]);
                //}

                //TODO return uBuild.interpretSearch(mState, result_array, search_offset);
                return false;
            }
            else
            {
                throw new OneWireIOException("Error communicating with adapter");
            }
        }

        /// <summary>
        /// Perform a 'strongAccess' with the provided 1-Wire address.
        /// 1-Wire Network has already been reset and the 'search'
        /// command sent before this is called.
        /// </summary>
        /// <param name="address">  device address to do strongAccess on </param>
        /// <param name="alarmOnly">  verify device is present and alarming if true
        /// </param>
        /// <returns>  true if device participated and was present
        ///         in the strongAccess search </returns>
        private bool blockIsPresent(byte[] address, bool alarmOnly)
        {
            byte[] send_packet = new byte[24];
            int i;

            // reset the 1-Wire
            reset();

            // send search command
            if (alarmOnly)
            {
//TODO                putByte(ALARM_SEARCH_CMD);
            }
            else
            {
//TODO                putByte(NORMAL_SEARCH_CMD);
            }

            // set all bits at first
            for (i = 0; i < 24; i++)
            {
                send_packet[i] = 0xFF;
            }

            // now set or clear apropriate bits for search
            for (i = 0; i < 64; i++)
            {
                Bit.arrayWriteBit(Bit.arrayReadBit(i, 0, address), (i + 1) * 3 - 1, 0, send_packet);
            }

            // send to 1-Wire Net
            dataBlock(send_packet, 0, 24);

            // check the results of last 8 triplets (should be no conflicts)
            int cnt = 56, goodbits = 0, tst, s;

            for (i = 168; i < 192; i += 3)
            {
                tst = (Bit.arrayReadBit(i, 0, send_packet) << 1) | Bit.arrayReadBit(i + 1, 0, send_packet);
                s = Bit.arrayReadBit(cnt++, 0, address);

                if (tst == 0x03) // no device on line
                {
                    goodbits = 0; // number of good bits set to zero

                    break; // quit
                }

                if (((s == 0x01) && (tst == 0x02)) || ((s == 0x00) && (tst == 0x01))) // correct bit
                {
                    goodbits++; // count as a good bit
                }
            }

            // check too see if there were enough good bits to be successful
            return (goodbits >= 8);
        }

	   //--------
	   //-------- Finding iButton/1-Wire device options
	   //--------

	   /// <summary>
	   /// This method does nothing in <code>DumbAdapter</code>.
	   /// </summary>
	   /// <seealso cref= #setNoResetSearch </seealso>
	   public override void setSearchOnlyAlarmingDevices()
	   {
            owState.searchOnlyAlarmingButtons = true;
	   }

	   /// <summary>
	   /// This method does nothing in <code>DumbAdapter</code>.
	   /// 
	   /// </summary>
	   public override void setNoResetSearch()
	   {
            owState.skipResetOnSearch = true;
	   }

	   /// <summary>
	   /// This method does nothing in <code>DumbAdapter</code>.
	   /// </summary>
	   /// <seealso cref= #setNoResetSearch </seealso>
	   public override void setSearchAllDevices()
	   {
            owState.searchOnlyAlarmingButtons = false;
            owState.skipResetOnSearch = false;
       }

       /// <summary>
       /// This method does nothing in <code>DumbAdapter</code>.
       /// </summary>
       /// <seealso cref=    #targetFamily </seealso>
       /// <seealso cref=    #targetFamily(byte[]) </seealso>
       /// <seealso cref=    #excludeFamily </seealso>
       /// <seealso cref=    #excludeFamily(byte[]) </seealso>
       public override void targetAllFamilies()
	   {

          // clear the include and exclude family search lists
          owState.searchIncludeFamilies = new byte[0];
          owState.searchExcludeFamilies = new byte[0];
	   }

	   /// <summary>
	   /// This method does nothing in <code>DumbAdapter</code>.
	   /// </summary>
	   /// <param name="family">   the code of the family type to target for searches </param>
	   /// <seealso cref=   com.dalsemi.onewire.utils.Address </seealso>
	   /// <seealso cref=    #targetAllFamilies </seealso>
	   public override void targetFamily(int family)
	   {

          // replace include family array with 1 element array
          owState.searchIncludeFamilies = new byte[1];
          owState.searchIncludeFamilies[0] = (byte)family;
	   }

	   /// <summary>
	   /// This method does nothing in <code>DumbAdapter</code>.
	   /// </summary>
	   /// <param name="family">  array of the family types to target for searches </param>
	   /// <seealso cref=   com.dalsemi.onewire.utils.Address </seealso>
	   /// <seealso cref=    #targetAllFamilies </seealso>
	   public override void targetFamily(byte[] family)
	   {

          // replace include family array with new array
          owState.searchIncludeFamilies = new byte[family.Length];

          Array.Copy(family, 0, owState.searchIncludeFamilies, 0, family.Length);
	   }

	   /// <summary>
	   /// This method does nothing in <code>DumbAdapter</code>.
	   /// </summary>
	   /// <param name="family">   the code of the family type NOT to target in searches </param>
	   /// <seealso cref=   com.dalsemi.onewire.utils.Address </seealso>
	   /// <seealso cref=    #targetAllFamilies </seealso>
	   public override void excludeFamily(int family)
	   {

            // replace exclude family array with 1 element array
            owState.searchExcludeFamilies = new byte[1];
            owState.searchExcludeFamilies[0] = (byte)family;
	   }

	   /// <summary>
	   /// This method does nothing in <code>DumbAdapter</code>.
	   /// </summary>
	   /// <param name="family">  array of family cods NOT to target for searches </param>
	   /// <seealso cref=   com.dalsemi.onewire.utils.Address </seealso>
	   /// <seealso cref=    #targetAllFamilies </seealso>
	   public override void excludeFamily(byte[] family)
	   {

            // replace exclude family array with new array
            owState.searchExcludeFamilies = new byte[family.Length];

            Array.Copy(family, 0, owState.searchExcludeFamilies, 0, family.Length);
        }

	   //--------
	   //-------- 1-Wire Network Semaphore methods
	   //--------

	   /// <summary>
	   /// This method does nothing in <code>DumbAdapter</code>.
	   /// </summary>
	   /// <param name="blocking"> <code>true</code> if want to block waiting
	   ///                 for an excluse access to the adapter </param>
	   /// <returns> <code>true</code> </returns>
	   public override bool beginExclusive(bool blocking)
	   {
			//DEBUG!!! RIGHT NOW THIS IS NOT IMPLEMENTED!!!
			return true;
	   }

	   /// <summary>
	   /// This method does nothing in <code>DumbAdapter</code>.
	   /// 
	   /// </summary>
	   public override void endExclusive()
	   {
			//DEBUG!!! RIGHT NOW THIS IS NOT IMPLEMENTED!!!
	   }

        /// <summary>
        /// Gets exclusive use of the 1-Wire to communicate with an iButton or
        /// 1-Wire Device if it is not already done.  Used to make methods
        /// thread safe.
        /// </summary>
        /// <exception cref="OneWireException"> on a setup error with the 1-Wire adapter </exception>
        private void beginLocalExclusive()
        {
            //// check if there is no such port
            //if (!PortOpen)
            //{
            //    throw new OneWireException("USerialAdapter: port not selected ");
            //}

            //// check if already have exclusive use
            //if (serial.haveExclusive())
            //{
            //    return;
            //}
            //else
            //{
            //    while (!haveLocalUse)
            //    {
            //        lock (syncObject)
            //        {
            //            haveLocalUse = serial.beginExclusive(false);
            //        }
            //        if (!haveLocalUse)
            //        {
            //            try
            //            {
            //                Thread.Sleep(50);
            //            }
            //            catch (Exception)
            //            {
            //            }
            //        }
            //    }
            //}
        }

        /// <summary>
        /// Relinquishes local exclusive control of the 1-Wire Network.  This
        /// just checks if we did our own 'beginExclusive' block and frees it.
        /// </summary>
        private void endLocalExclusive()
        {
            //lock (syncObject)
            //{
            //    if (haveLocalUse)
            //    {
            //        haveLocalUse = false;

            //        serial.endExclusive();
            //    }
            //}
        }

	   //--------
	   //-------- Primitive 1-Wire Network data methods
	   //--------

	   /// <summary>
	   /// Sends a bit to the 1-Wire Network.
	   /// This method does nothing in <code>DumbAdapter</code>.
	   /// </summary>
	   /// <param name="bitValue">  the bit value to send to the 1-Wire Network. </param>
	   public override void putBit(bool bitValue)
	   {
		  //this will not be implemented
	   }

	   /// <summary>
	   /// Gets a bit from the 1-Wire Network.
	   /// This method does nothing in <code>DumbAdapter</code>.
	   /// </summary>
	   /// <returns>  <code>true</code> </returns>
	   public override bool getBit
	   {
		   get
		   {
			  //this will not be implemented
			  return true;
		   }
	   }

	   /// <summary>
	   /// This method does nothing in <code>DumbAdapter</code>.
	   /// </summary>
	   /// <param name="byteValue">  the byte value to send to the 1-Wire Network. </param>
	   public override void putByte(int byteValue)
	   {
		  //this will not be implemented
	   }

	   /// <summary>
	   /// This method does nothing in <code>DumbAdapter</code>.
	   /// </summary>
	   /// <returns> the value 0x0ff </returns>
	   public override int Byte
	   {
		   get
		   {
			  //this will not be implemented
			  return 0x0ff;
		   }
	   }

	   /// <summary>
	   /// This method does nothing in <code>DumbAdapter</code>.
	   /// </summary>
	   /// <param name="len">  length of data bytes to receive
	   /// </param>
	   /// <returns> a new byte array of length <code>len</code> </returns>
	   public override byte[] getBlock(int len)
	   {
		  //this will not be implemented
		  return new byte[len];
	   }

	   /// <summary>
	   /// This method does nothing in <code>DumbAdapter</code>.
	   /// </summary>
	   /// <param name="arr">     array in which to write the received bytes </param>
	   /// <param name="len">     length of data bytes to receive </param>
	   public override void getBlock(byte[] arr, int len)
	   {
		  //this will not be implemented
	   }

	   /// <summary>
	   /// This method does nothing in <code>DumbAdapter</code>.
	   /// </summary>
	   /// <param name="arr">     array in which to write the received bytes </param>
	   /// <param name="off">     offset into the array to start </param>
	   /// <param name="len">     length of data bytes to receive </param>
	   public override void getBlock(byte[] arr, int off, int len)
	   {
		  //this will not be implemented
	   }

	   /// <summary>
	   /// This method does nothing in <code>DumbAdapter</code>.
	   /// </summary>
	   /// <param name="dataBlock">  array of data to transfer to and from the 1-Wire Network. </param>
	   /// <param name="off">        offset into the array of data to start </param>
	   /// <param name="len">        length of data to send / receive starting at 'off' </param>
	   public override void dataBlock(byte[] dataBlock, int off, int len)
	   {
		  //this will not be implemented
	   }

        /// <summary>
        /// Sends a Reset to the 1-Wire Network.
        /// </summary>
        /// <returns>  the result of the reset. Potential results are:
        /// <ul>
        /// <li> 0 (RESET_NOPRESENCE) no devices present on the 1-Wire Network.
        /// <li> 1 (RESET_PRESENCE) normal presence pulse detected on the 1-Wire
        ///        Network indicating there is a device present.
        /// <li> 2 (RESET_ALARM) alarming presence pulse detected on the 1-Wire
        ///        Network indicating there is a device present and it is in the
        ///        alarm condition.  This is only provided by the DS1994/DS2404
        ///        devices.
        /// <li> 3 (RESET_SHORT) inticates 1-Wire appears shorted.  This can be
        ///        transient conditions in a 1-Wire Network.  Not all adapter types
        ///        can detect this condition.
        /// </ul>
        /// </returns>
        /// <exception cref="OneWireIOException"> on a 1-Wire communication error </exception>
        /// <exception cref="OneWireException"> on a setup error with the 1-Wire adapter </exception>
        public override int reset()
        {
            try
            {

                // acquire exclusive use of the port
                beginLocalExclusive();

                // make sure adapter is present
                if (uAdapterPresent())
                {

                    // check for pending power conditions
                    if (owState.oneWireLevel != LEVEL_NORMAL)
                    {
                        setPowerNormal();
                    }

                    //// build a message to read the baud rate from the U brick
                    //uBuild.restart();

                    //int reset_offset = uBuild.oneWireReset();

                    //// send and receive
                    //byte[] result_array = uTransaction(uBuild);

                    //// check the result
                    //if (result_array.Length == (reset_offset + 1))
                    //{
                    //    return uBuild.interpretOneWireReset(result_array[reset_offset]);
                    //}
                    //else
                    //{
                    //    throw new OneWireIOException("USBAdapter-reset: no return byte form 1-Wire reset");
                    //}
                    return 0; //TODO
                }
                else
                {
                    throw new OneWireIOException("Error communicating with adapter");
                }
            }
            catch (IOException ioe)
            {
                throw new OneWireIOException(ioe.ToString());
            }
            finally
            {

                // release local exclusive use of port
                endLocalExclusive();
            }
        }

        //--------
        //-------- 1-Wire Network power methods
        //--------

        /// <summary>
        /// This method does nothing in <code>DumbAdapter</code>.
        /// </summary>
        /// <param name="timeFactor">
        /// <ul>
        /// <li>   0 (DELIVERY_HALF_SECOND) provide power for 1/2 second.
        /// <li>   1 (DELIVERY_ONE_SECOND) provide power for 1 second.
        /// <li>   2 (DELIVERY_TWO_SECONDS) provide power for 2 seconds.
        /// <li>   3 (DELIVERY_FOUR_SECONDS) provide power for 4 seconds.
        /// <li>   4 (DELIVERY_SMART_DONE) provide power until the
        ///          the device is no longer drawing significant power.
        /// <li>   5 (DELIVERY_INFINITE) provide power until the
        ///          setPowerNormal() method is called.
        /// </ul> </param>
        public override int PowerDuration
	   {
		   set
		   {
		   }
	   }

	   /// <summary>
	   /// This method does nothing in <code>DumbAdapter</code>.
	   /// </summary>
	   /// <param name="changeCondition">
	   /// <ul>
	   /// <li>   0 (CONDITION_NOW) operation should occur immediately.
	   /// <li>   1 (CONDITION_AFTER_BIT) operation should be pending
	   ///           execution immediately after the next bit is sent.
	   /// <li>   2 (CONDITION_AFTER_BYTE) operation should be pending
	   ///           execution immediately after next byte is sent.
	   /// </ul>
	   /// </param>
	   /// <returns> <code>true</code> </returns>
	   public override bool startPowerDelivery(int changeCondition)
	   {
		  return true;
	   }

	   /// <summary>
	   /// This method does nothing in <code>DumbAdapter</code>.
	   /// </summary>
	   /// <param name="timeFactor">
	   /// <ul>
	   /// <li>   7 (DELIVERY_EPROM) provide program pulse for 480 microseconds
	   /// <li>   5 (DELIVERY_INFINITE) provide power until the
	   ///          setPowerNormal() method is called.
	   /// </ul> </param>
	   public override int ProgramPulseDuration
	   {
		   set
		   {
		   }
	   }

	   /// <summary>
	   /// This method does nothing in <code>DumbAdapter</code>.
	   /// </summary>
	   /// <param name="changeCondition">
	   /// <ul>
	   /// <li>   0 (CONDITION_NOW) operation should occur immediately.
	   /// <li>   1 (CONDITION_AFTER_BIT) operation should be pending
	   ///           execution immediately after the next bit is sent.
	   /// <li>   2 (CONDITION_AFTER_BYTE) operation should be pending
	   ///           execution immediately after next byte is sent.
	   /// </ul>
	   /// </param>
	   /// <returns> <code>true</code> </returns>
	   public override bool startProgramPulse(int changeCondition)
	   {
		   return true;
	   }

	   /// <summary>
	   /// This method does nothing in <code>DumbAdapter</code>.
	   /// 
	   /// </summary>
	   public override void startBreak()
	   {
	   }

	   /// <summary>
	   /// This method does nothing in <code>DumbAdapter</code>.
	   /// 
	   /// </summary>
	   public override void setPowerNormal()
	   {
            try
            {
                Debugger.Break();

                // acquire exclusive use of the port
                beginLocalExclusive();

                if (owState.oneWireLevel == LEVEL_POWER_DELIVERY)
                {

                    // make sure adapter is present
                    if (uAdapterPresent())
                    {

                        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
                        // shughes - 8-28-2003
                        // Fixed the Set Power Level Normal problem where adapter
                        // is left in a bad state.  Removed bad fix: extra getBit()
                        // SEE BELOW!
                        // stop pulse command
                        uBuild.sendCommand(UPacketBuilder.FUNCTION_STOP_PULSE, true);

                        // start pulse with no prime
                        uBuild.sendCommand(UPacketBuilder.FUNCTION_5VPULSE_NOW, false);
                        //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

                        // add the command to stop the pulse
                        int pulse_response_offset = uBuild.sendCommand(UPacketBuilder.FUNCTION_STOP_PULSE, true);

                        // send and receive
                        //byte[] result_array = uTransaction(uBuild);
                        byte[] result_array = new byte[pulse_response_offset+1];

                        // check the result
                        if (result_array.Length == (pulse_response_offset + 1))
                        {
                            owState.oneWireLevel = LEVEL_NORMAL;

                            //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
                            // shughes - 8-28-2003
                            // This is a bad "fix", it was needed when we were causing
                            // a bad condition.  Instead of fixing it here, we should
                            // fix it where we were causing it..  Which we did!
                            // SEE ABOVE!
                            //getBit();
                            //\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
                        }
                        else
                        {
                            throw new OneWireIOException("Did not get a response back from stop power delivery");
                        }
                    }
                }
                else if (owState.oneWireLevel == LEVEL_BREAK)
                {

                    //// restore power
                    //serial.DTR = true;
                    //serial.RTS = true;

                    //// wait for power to come up
                    //sleep(300);

                    // set the level state
                    owState.oneWireLevel = LEVEL_NORMAL;

                    // set the DS2480 to the correct mode and verify
                    adapterPresent = false;

                    if (!uAdapterPresent())
                    {
                        throw new OneWireIOException("Did not get a response back from adapter after break");
                    }
                }
            }
            catch (IOException ioe)
            {
                throw new OneWireIOException(ioe.ToString());
            }
            finally
            {

                // release local exclusive use of port
                endLocalExclusive();
            }
        }

        //--------
        //-------- 1-Wire Network operation append methods
        //--------

        /// <summary>
        /// Add the command to reset the OneWire at the current speed.
        /// </summary>
        ///  <returns> the number offset in the return packet to get the
        ///          result of this operation </returns>
        public virtual int oneWireReset()
        {
            SendCommand(
                Ds2490.CMD_TYPE.COMM,
                Ds2490.COMM._1_WIRE_RESET | Ds2490.COMM.F | Ds2490.COMM.IM | Ds2490.COMM.SE,
                uState.uSpeedMode,
                "One-Wire Reset");

            //// append the reset command at the current speed
            //packet.writer.Write((byte)(FUNCTION_RESET | uState.uSpeedMode)); //TODO .Append


            //// check if not streaming resets
            //if (!uState.streamResets)
            //{
            //    newPacket();
            //}

            //// check for 2480 wait on extra bytes packet
            //if (uState.longAlarmCheck && ((uState.uSpeedMode == UsbAdapterState.USPEED_REGULAR) || (uState.uSpeedMode == UsbAdapterState.USPEED_FLEX)))
            //{
            //    newPacket();
            //}

            //return totalReturnLength - 1;
            return 5;
        }

        //--------
        //-------- 1-Wire Network speed methods
        //--------

        /// <summary>
        /// This method does nothing in <code>DumbAdapter</code>.
        /// </summary>
        /// <param name="speed">
        /// <ul>
        /// <li>     0 (SPEED_REGULAR) set to normal communciation speed
        /// <li>     1 (SPEED_FLEX) set to flexible communciation speed used
        ///            for long lines
        /// <li>     2 (SPEED_OVERDRIVE) set to normal communciation speed to
        ///            overdrive
        /// <li>     3 (SPEED_HYPERDRIVE) set to normal communciation speed to
        ///            hyperdrive
        /// <li>    >3 future speeds
        /// </ul>
        ///  </param>
        public override int Speed
	   {
		   set
		   {
			   sp = value;
		   }
		   get
		   {
			  return sp;
		   }
	   }

	   private int sp = 0;


	   //--------
	   //-------- Misc
	   //--------

	   /// <summary>
	   /// Gets the container from this adapter whose address matches the address of a container
	   /// in the <code>DumbAdapter</code>'s internal <code>java.util.Vector</code>.
	   /// </summary>
	   /// <param name="address">  device address with which to find a container
	   /// </param>
	   /// <returns>  The <code>OneWireContainer</code> object, or <code>null</code> if no match could be found. </returns>
	   /// <seealso cref=   com.dalsemi.onewire.utils.Address </seealso>
	  // public override OneWireContainer getDeviceContainer(byte[] address)
	  // {
			//long addr = Address.toLong(address);
			//lock (containers)
			//{
			//	for (int i = 0;i < containers.Count;i++)
			//	{
			//		if (((OneWireContainer)containers[i]).AddressAsLong == addr)
			//		{
			//			return (OneWireContainer)containers[i];
			//		}
			//	}
			//}
			//return null;

	  // }

	   /// <summary>
	   /// Gets the container from this adapter whose address matches the address of a container
	   /// in the <code>DumbAdapter</code>'s internal <code>java.util.Vector</code>.
	   /// </summary>
	   /// <param name="address">  device address with which to find a container
	   /// </param>
	   /// <returns>  The <code>OneWireContainer</code> object, or <code>null</code> if no match could be found. </returns>
	   /// <seealso cref=   com.dalsemi.onewire.utils.Address </seealso>
	   //public override OneWireContainer getDeviceContainer(long address)
	   //{
		  //return getDeviceContainer(Address.toByteArray(address));
	   //}

	   /// <summary>
	   /// Gets the container from this adapter whose address matches the address of a container
	   /// in the <code>DumbAdapter</code>'s internal <code>java.util.Vector</code>.
	   /// </summary>
	   /// <param name="address">  device address with which to find a container
	   /// </param>
	   /// <returns>  The <code>OneWireContainer</code> object, or <code>null</code> if no match could be found. </returns>
	   /// <seealso cref=   com.dalsemi.onewire.utils.Address </seealso>
	   //public override OneWireContainer getDeviceContainer(string address)
	   //{
		  //return getDeviceContainer(Address.toByteArray(address));
	   //}

	   /// <summary>
	   /// Returns a <code>OneWireContainer</code> object using the current 1-Wire network address.
	   /// The internal state of the port adapter keeps track of the last
	   /// address found and is able to create container objects from this
	   /// state.
	   /// </summary>
	   /// <returns>  the <code>OneWireContainer</code> object </returns>
	   public override OneWireContainer DeviceContainer
	   {
		   get
		   {
    
			  // Mask off the upper bit.
			  byte[] address = new byte [8];
    
			  getAddress(address);
    
			  return getDeviceContainer(address);
		   }
	   }

	   /// <summary>
	   /// Checks to see if the family found is in the desired
	   /// include group.
	   /// </summary>
	   /// <returns>  <code>true</code> if in include group </returns>
	   protected internal override bool isValidFamily(byte[] address)
	   {
		  byte familyCode = address [0];

		  if (exclude != null)
		  {
			 for (int i = 0; i < exclude.Length; i++)
			 {
				if (familyCode == exclude [i])
				{
				   return false;
				}
			 }
		  }

		  if (include != null)
		  {
			 for (int i = 0; i < include.Length; i++)
			 {
				if (familyCode == include [i])
				{
				   return true;
				}
			 }

			 return false;
		  }

		  return true;
	   }
	}

}