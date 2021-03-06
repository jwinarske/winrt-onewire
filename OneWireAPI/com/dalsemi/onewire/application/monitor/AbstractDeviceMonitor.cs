﻿using System;
using System.Collections.Generic;

/*---------------------------------------------------------------------------
 * Copyright (C) 2002 Dallas Semiconductor Corporation, All Rights Reserved.
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

namespace com.dalsemi.onewire.application.monitor
{
    using com.dalsemi.onewire.adapter;
    using com.dalsemi.onewire.container;
    using com.dalsemi.onewire.utils;

    /// <summary>
    /// <P>Abstract super-class for 1-Wire Monitors, a optionally-threadable
    /// object for searching 1-Wire networks.  If this object is not run in it's own
    /// thread, it is possible to perform single-step searches by calling the search </summary>
    /// method directly {<seealso cref= #search(Vector, Vector)}.  The monitor will generate
    /// events for device arrivals, device departures, and exceptions from the
    /// DSPortAdapter.</P>
    ///
    /// <P>In a touch-contact environment, it is not suitable to say that a
    /// device has "departed" because it was missing for one cycle of searching.
    /// In the time it takes to get an iButton into a blue-dot receptor, the
    /// monitor could have generated a handful of arrival and departure events.  To
    /// circumvent this problem, the device monitor keeps a "missing state count" for
    /// each device on the network.  Each search cycle that passes where the device
    /// is missing causes it's "missing state count" to be incremented.  Once the
    /// device's "missing state count" is equal to the "max state count" </seealso>
    /// {<seealso cref= #getMaxStateCount()}, a departure event is generated for the device.
    /// If the 1-Wire Network is not in a touch environment, it may be unnecessary
    /// to use this "missing state count".  In those instances, setting the state </seealso>
    /// count to 1 will disable the feature {<seealso cref= #setMaxStateCount(int)}.</P>
    ///
    /// <P>Similarly, the reporting of exceptions could be spurious in a
    /// touch-contact environment.  Instead of reporting the exception on each
    /// failed search attempt, the monitor will default to retrying the search a </seealso>
    /// handful of times {<seealso cref= #getMaxErrorCount()} before finally reporting the
    /// exception.  To disable this feature, set the max error count to 1 </seealso>
    /// {<seealso cref= #setMaxErrorCount(int)}.</P>
    ///
    /// <P>To receive events, an object must implement the
    /// <code>DeviceMonitorEventListener</code> interface </seealso>
    /// {<seealso cref= DeviceMonitorEventListener} and must be added to </seealso>
    /// the list of listeners {<seealso cref= #addDeviceMonitorEventListener}.</P>
    ///
    /// @author SH
    /// @version 1.00 </seealso>
    public abstract class AbstractDeviceMonitor
    {
        //--------
        //-------- Constants
        //--------

        /// <summary>
        /// object used for synchronization </summary>
        protected internal readonly object sync_flag = new object();

        /// <summary>
        /// Addresses of all current devices, mapped to their state count </summary>
        protected internal Dictionary<long, int> deviceAddressHash =
             new Dictionary<long, int>();

        /// <summary>
        /// hashtable for holding device containers, static to keep only a
        /// single instance of each OneWireContainer.
        /// </summary>
        protected internal static readonly Dictionary<long, OneWireContainer> deviceContainerHash =
             new Dictionary<long, OneWireContainer>();

        /// <summary>
        /// Listeners who receive notification of events generated by this
        /// device Monitor
        /// </summary>
        protected internal readonly List<DeviceMonitorEventListener> listeners =
             new List<DeviceMonitorEventListener>();

        //--------
        //-------- Variables
        //--------

        /// <summary>
        /// Number of searches that a button should be "missing" before it is removed </summary>
        protected internal int max_state_count = 3;

        /// <summary>
        /// Number of searches that an error occurs before a dialog is displayed </summary>
        protected internal int max_error_count = 6;

        /// <summary>
        /// Flag for overall thread running state </summary>
        protected internal volatile bool keepRunning = true, hasCompletelyStopped = false;

        /// <summary>
        /// Flag to indicate thread has begin to run </summary>
        protected internal volatile bool startRunning = true;

        /// <summary>
        /// Flag to indicate thread is running now </summary>
        protected internal volatile bool isRunning = false;

        /// <summary>
        /// the adapter to search for devices </summary>
        protected internal DSPortAdapter adapter = null;

        /// <summary>
        /// The device monitor will internally cache OneWireContainer objects for each
        /// 1-Wire device.  Use this method to clean up all stale container objects.
        /// A stale container object is a OneWireContainer object which references a
        /// 1-Wire device address which has not been seen by a recent search.
        /// This will be essential in a touch-contact environment which could run
        /// for some time and needs to conserve memory.
        /// </summary>
        public virtual void cleanUpStaleContainerReferences()
        {
            lock (deviceContainerHash)
            {
                System.Collections.IEnumerator e = deviceContainerHash.Keys.GetEnumerator();
                while (e.MoveNext())
                {
                    long o = (long)e.Current;
                    if (!deviceAddressHash.ContainsKey(o))
                    {
                        deviceContainerHash.Remove(o);
                    }
                }
            }
        }

        /// <summary>
        /// The device monitor will internally cache OWPath objects for each
        /// 1-Wire device.  Use this method to clean up all stale OWPath objects.
        /// A stale path object is a OWPath which references a branching path to a
        /// 1-Wire device address which has not been seen by a recent search.
        /// This will be essential in a touch-contact environment which could run
        /// for some time and needs to conserve memory.
        /// </summary>
        public virtual void cleanUpStalePathReferences()
        {
            // no-op by default.  Only NetworkDeviceMonitor uses paths
        }

        /// <summary>
        /// Resets this device monitor.  All known devices will be marked as
        /// "departed" and departure events will be fired.
        /// </summary>
        public virtual void resetSearch()
        {
            lock (sync_flag)
            {
                // fire departures for all devices
                if (deviceAddressHash.Count > 0 && listeners.Count > 0)
                {
                    List<long> v = new List<long>(deviceAddressHash.Count);
                    System.Collections.IEnumerator e = deviceAddressHash.Keys.GetEnumerator();
                    while (e.MoveNext())
                    {
                        v.Add((long)e.Current);
                    }
                    fireDepartureEvent(adapter, v);
                }

                deviceAddressHash.Clear();
            }
        }

        /// <summary>
        /// The number of searches that a button should be "missing"
        /// before it is removed.
        /// </summary>
        /// <returns> The number of searches that a button should be "missing"
        /// before it is removed. </returns>
        public virtual int MaxStateCount
        {
            get
            {
                return this.max_state_count;
            }
            set
            {
                if (value <= 0)
                {
                    throw new System.ArgumentException("State Count must be greater than 0");
                }

                this.max_state_count = value;
            }
        }

        /// <summary>
        /// Number of searches that an error occurs before listener's are notified
        /// </summary>
        /// <returns> Number of searches that an error occurs before listener's
        /// are notified </returns>
        public virtual int MaxErrorCount
        {
            get
            {
                return this.max_error_count;
            }
            set
            {
                if (value <= 0)
                {
                    throw new System.ArgumentException("Error Count must be greater than 0");
                }

                this.max_error_count = value;
            }
        }

        /// <summary>
        /// Sets this monitor to search a new DSPortAdapter
        /// </summary>
        /// <param name="the"> DSPortAdapter this monitor should search </param>
        public abstract DSPortAdapter Adapter { get; set; }

        //--------
        //-------- Monitor methods
        //--------

        /// <summary>
        /// Performs a search of the 1-Wire network
        /// </summary>
        /// <param name="arrivals"> A vector of Long objects, represent new arrival addresses. </param>
        /// <param name="departures"> A vector of Long objects, represent departed addresses. </param>
        public abstract void search(List<long> arrivals, List<long> departures);

        /// <summary>
        /// Pause this monitor
        /// </summary>
        /// <param name="blocking"> if true, this method will block until the monitor is paused.
        /// @returns true if the monitor was successfully paused. </param>
        public virtual bool pauseMonitor(bool blocking)
        {
            // clear the start flag
            lock (sync_flag)
            {
                if (hasCompletelyStopped || (!startRunning && !isRunning))
                {
                    return true;
                }

                startRunning = false;
            }

            // wait until it is paused or until timeout
            int i = 0;
            while (isRunning && (blocking || (i++) < 100))
            {
                msSleep(10);
            }

            return !isRunning;
        }

        /// <summary>
        /// Resume this monitor
        /// </summary>
        /// <param name="blocking"> if true, this method will block until the monitor is resumed.
        /// @returns true if the monitor was successfully resumed. </param>
        public virtual bool resumeMonitor(bool blocking)
        {
            // set the start flag
            lock (sync_flag)
            {
                if (hasCompletelyStopped)
                {
                    return false;
                }
                if (startRunning && isRunning)
                {
                    return true;
                }

                startRunning = true;
            }

            // wait until it is running
            int i = 0;
            while (!isRunning && (blocking || (i++) < 100))
            {
                msSleep(10);
            }

            return isRunning;
        }

        /// <summary>
        /// Check if this monitor is running.
        /// </summary>
        /// <returns> <CODE>true</CODE> if monitor is running </returns>
        public virtual bool MonitorRunning
        {
            get
            {
                return isRunning;
            }
        }

        /// <summary>
        /// Kill this monitor.  Wait util this
        /// thread is no longer alive (with timeout).
        /// </summary>
        public virtual void killMonitor()
        {
            // clear the running flags
            lock (sync_flag)
            {
                keepRunning = false;
                startRunning = false;
            }

            // wait until the thread is no longer running, with a timeout
            int i = 0;
            while (!hasCompletelyStopped && i < 100)
            {
                msSleep(20);
            }
        }

        /// <summary>
        /// Monitor run method that performs a periodic search
        /// of the entire 1-Wire network.  Listeners
        /// that have registered are notified when changes in the network
        /// are detected.
        /// </summary>
        public virtual void run()
        {
            lock (sync_flag)
            {
                hasCompletelyStopped = false;
            }

            List<long> arrivals = new List<long>(), departures = new List<long>();
            while (keepRunning)
            {
                if (startRunning)
                {
                    // set is now running
                    lock (sync_flag)
                    {
                        isRunning = true;
                    }

                    // erase previous arrivals and departures
                    arrivals.Clear();
                    departures.Clear();

                    // number of times an error occurred during 1-Wire search
                    int errorCount = 0;
                    bool done = false;
                    while (!done)
                    {
                        try
                        {
                            // search for new devices, remove stale device entries
                            search(arrivals, departures);
                            done = true;
                        }
                        catch (System.Exception e)
                        {
                            if (++errorCount == max_error_count)
                            {
                                fireException(adapter, e);
                                done = true;
                            }
                        }
                    }

                    // sleep to give other threads a chance at this network
                    msSleep(200);
                }
                else
                {
                    // not running so clear flag
                    lock (sync_flag)
                    {
                        isRunning = false;
                    }
                    msSleep(200);
                }
            }
            // not running so clear flag
            lock (sync_flag)
            {
                isRunning = false;
                hasCompletelyStopped = true;
            }
        }

        //--------
        //-------- Utility methods
        //--------

        /// <summary>
        /// Sleep for the specified number of milliseconds
        /// </summary>
        /// <param name="msTime"> number of milliseconds to sleep </param>
        protected internal virtual void msSleep(long msTime)
        {
            Thread.Sleep(msTime);
        }

        //--------
        //-------- Event methods
        //--------

        /// <summary>
        /// Add a listener, to be notified of arrivals, departures, and exceptions.
        /// </summary>
        /// <param name="dmel"> Listener of monitor events. </param>
        public virtual void addDeviceMonitorEventListener(DeviceMonitorEventListener dmel)
        {
            if (dmel != null)
            {
                this.listeners.Add(dmel);
            }
        }

        /// <summary>
        /// Notify the listeners of the arrival event
        /// </summary>
        /// <param name="address"> Vector of Long objects representing the address of new
        /// arrivals. </param>
        protected internal virtual void fireArrivalEvent(DSPortAdapter adapter, List<long> address)
        {
            DeviceMonitorEvent dme = new DeviceMonitorEvent(DeviceMonitorEvent.ARRIVAL, this, adapter, address);
            for (int i = 0; i < listeners.Count; i++)
            {
                DeviceMonitorEventListener listener = (DeviceMonitorEventListener)listeners[i];
                listener.deviceArrival(dme);
            }
        }

        /// <summary>
        /// Notify the listeners of the departure event
        /// </summary>
        /// <param name="address"> Vector of Long objects representing the address of
        /// departed devices. </param>
        protected internal virtual void fireDepartureEvent(DSPortAdapter adapter, List<long> address)
        {
            DeviceMonitorEvent dme = new DeviceMonitorEvent(DeviceMonitorEvent.DEPARTURE, this, adapter, address);

            for (int i = 0; i < listeners.Count; i++)
            {
                DeviceMonitorEventListener listener = (DeviceMonitorEventListener)listeners[i];
                listener.deviceDeparture(dme);
            }
        }

        /// <summary>
        /// Notify the listeners of the exception
        /// </summary>
        /// <param name="ex"> The exception that occurred. </param>
        private void fireException(DSPortAdapter adapter, Exception ex)
        {
            for (int i = 0; i < listeners.Count; i++)
            {
                ((DeviceMonitorEventListener)listeners[i]).networkException(new DeviceMonitorException(this, adapter, ex));
            }
        }

        /// <summary>
        /// Returns the OWPath of the device with the given address.
        /// </summary>
        /// <param name="address"> a byte array representing the address of the device </param>
        /// <returns> The OWPath representing the network path to the device. </returns>
        public virtual OWPath getDevicePath(byte[] address)
        {
            return getDevicePath(Address.toLong(address));
        }

        /// <summary>
        /// Returns the OWPath of the device with the given address.
        /// </summary>
        /// <param name="address"> a string representing the address of the device </param>
        /// <returns> The OWPath representing the network path to the device. </returns>
        public virtual OWPath getDevicePath(string address)
        {
            return getDevicePath(Address.toLong(address));
        }

        /// <summary>
        /// Returns the OWPath of the device with the given address.
        /// </summary>
        /// <param name="address"> a Long object representing the address of the device </param>
        /// <returns> The OWPath representing the network path to the device. </returns>
        public abstract OWPath getDevicePath(long address); //?

        /// <summary>
        /// Returns all addresses known by this monitor as an Enumeration of Long
        /// objects.
        /// </summary>
        /// <returns> Enumeration of Long objects </returns>
        public virtual System.Collections.IEnumerator AllAddresses
        {
            get
            {
                return deviceAddressHash.Keys.GetEnumerator();
            }
        }

        //--------
        //-------- Static methods
        //--------

        /// <summary>
        /// Returns the OneWireContainer object of the device with the given address.
        /// </summary>
        /// <param name="adapter"> The DSPortAdapter that the device is connected to. </param>
        /// <param name="address"> a byte array representing the address of the device </param>
        /// <returns> The specific OneWireContainer object of the device </returns>
        public static OneWireContainer getDeviceContainer(DSPortAdapter adapter, byte[] address)
        {
            return getDeviceContainer(adapter, Address.toLong(address));
        }

        /// <summary>
        /// Returns the OneWireContainer object of the device with the given address.
        /// </summary>
        /// <param name="adapter"> The DSPortAdapter that the device is connected to. </param>
        /// <param name="address"> a String representing the address of the device </param>
        /// <returns> The specific OneWireContainer object of the device </returns>
        public static OneWireContainer getDeviceContainer(DSPortAdapter adapter, string address)
        {
            return getDeviceContainer(adapter, Address.toLong(address));
        }

        /// <summary>
        /// Returns the OneWireContainer object of the device with the given address.
        /// </summary>
        /// <param name="adapter"> The DSPortAdapter that the device is connected to. </param>
        /// <param name="address"> a Long object representing the address of the device </param>
        /// <returns> The specific OneWireContainer object of the device </returns>
        public static OneWireContainer getDeviceContainer(DSPortAdapter adapter, long longAddress)
        {
            lock (deviceContainerHash)
            {
                if (!deviceContainerHash.ContainsKey(longAddress))
                {
                    var o = adapter.getDeviceContainer(longAddress);
                    putDeviceContainer(longAddress, o);
                    return o;
                }
                else
                {
                    return deviceContainerHash[longAddress];
                }
            }
        }

        /// <summary>
        /// Sets the OneWireContainer object of the device with the given address.
        /// </summary>
        /// <param name="address"> a byte array object representing the address of the device </param>
        /// <param name="owc"> The specific OneWireContainer object of the device </param>
        public static void putDeviceContainer(byte[] address, OneWireContainer owc)
        {
            putDeviceContainer(Address.toLong(address), owc);
        }

        /// <summary>
        /// Sets the OneWireContainer object of the device with the given address.
        /// </summary>
        /// <param name="address"> a String object representing the address of the device </param>
        /// <param name="owc"> The specific OneWireContainer object of the device </param>
        public static void putDeviceContainer(string address, OneWireContainer owc)
        {
            putDeviceContainer(Address.toLong(address), owc);
        }

        /// <summary>
        /// Sets the OneWireContainer object of the device with the given address.
        /// </summary>
        /// <param name="address"> a Long object representing the address of the device </param>
        /// <param name="owc"> The specific OneWireContainer object of the device </param>
        public static void putDeviceContainer(long longAddress, OneWireContainer owc)
        {
            lock (deviceContainerHash)
            {
                deviceContainerHash[longAddress] = owc;
            }
        }
    }
}