﻿/*
MIT License - PowerPing 

Copyright (c) 2021 Matthew Carney

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

using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace PowerPing
{
    /// <summary>
    /// Stores the results of a ping operation
    /// </summary>
    public class PingResults
    {
        // Properties
        public DateTime StartTime { get; private set; }                             // Time operation started at 
        public TimeSpan TotalRunTime { get { return m_OperationTimer.Elapsed; } }   // Total ping operation runtime
        public ulong Sent { get; private set; }                                     // Number of sent ping packets
        public ulong Received { get; private set; }                                 // Number of received packets
        public ulong Lost { get; private set; }                                     // Amount of lost packets
        public double MaxTime { get; private set; }                                 // Highest ping reply time
        public double MinTime { get; private set; }                                 // Lowest ping reply time
        public double AvgTime { get; private set; }                                 // Average reply time
        public double CurrTime { get; private set; }                                // Most recent packet response time
        public ulong ErrorPackets { get; private set; }                             // Number of Error packet received
        public ulong GoodPackets { get; private set; }                              // Number of good replies received
        public ulong OtherPackets { get; private set; }                             // Number of other packet types received
        public bool HasOverflowed { get; set; }                                     // Specifies if any of the results have overflowed
        public bool ScanWasCanceled { get; set; }                                   // Whether the scan was canceled early

        private readonly Stopwatch m_OperationTimer = new Stopwatch();              // Used to time total time spent doing operation
        private double m_ResponseTimeSum = 0;                                       // Sum of all reply times (used to work out general average 

        public PingResults()
        {
            // Default properties
            Sent = 0;
            Received = 0;
            Lost = 0;
            MaxTime = 0;
            MinTime = 0;
            AvgTime = 0;
            CurrTime = -1;
            ErrorPackets = 0;
            GoodPackets = 0;
            OtherPackets = 0;

            HasOverflowed = false;

            // Get local start time
            StartTime = DateTime.Now;

            // Start timing operation
            m_OperationTimer.Start();
        }

        public void SaveResponseTime(double time)
        {
            if (time == -1f) {
                CurrTime = 0;
                return;
            }

            // BUG: Converting from long to double might be causing precisson loss
            // Check response time against current max and min
            if (time > MaxTime) {
                MaxTime = time;
            }

            if (time < MinTime || MinTime == 0) {
                MinTime = time;
            }

            try {
                // Work out average
                m_ResponseTimeSum += time;
                AvgTime = m_ResponseTimeSum / Received; // Avg = Total / Count
            } catch (OverflowException) {
                HasOverflowed = true;
            }
            CurrTime = time;
        }
        public void CountPacketType(int type)
        {
            try
            {
                if (type == 0) {
                    GoodPackets++;
                } else if (type == 3 || type == 4 || type == 5 || type == 11) {
                    ErrorPackets++;
                } else {
                    OtherPackets++;
                }
            } catch (OverflowException) {
                HasOverflowed = true;
            }
        }

        public void IncrementSentPackets()
        {
            try {
                Sent++;
            } catch (OverflowException) {
                HasOverflowed = true;
            }
        }
        public void IncrementReceivedPackets()
        {
            try {
                Received++;
            } catch (OverflowException) {
                HasOverflowed = true;
            }
        }
        public void IncrementLostPackets()
        {
            try {
                Lost++;
            } catch (OverflowException) {
                HasOverflowed = true;
            }
        }
    }

}