﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EmDataSaver {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class emstrings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal emstrings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("EmDataSaver.emstrings", typeof(emstrings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You&apos;ve selected an archive of 3-second values which lasted more than {0} hours. We recommend to decrease the time interval. Continue with such interval?.
        /// </summary>
        internal static string archive_more_than_limit {
            get {
                return ResourceManager.GetString("archive_more_than_limit", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to read the AVG archive at {0}..
        /// </summary>
        internal static string avg_reading_failed {
            get {
                return ResourceManager.GetString("avg_reading_failed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to EmWorkNet7 Image Files|*.{0}|All Files|*.*.
        /// </summary>
        internal static string dialog_filter {
            get {
                return ResourceManager.GetString("dialog_filter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to read the DNS archive at {0}..
        /// </summary>
        internal static string dns_reading_failed {
            get {
                return ResourceManager.GetString("dns_reading_failed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The device firmware version is obsolete. The program cannot communicate with it. Apply to the manufacturer to a get new firmware version..
        /// </summary>
        internal static string etpqp_version_is_old {
            get {
                return ResourceManager.GetString("etpqp_version_is_old", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The device has been turned on.
        /// </summary>
        internal static string event_journal_0_text {
            get {
                return ResourceManager.GetString("event_journal_0_text", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The device has been turned off.
        /// </summary>
        internal static string event_journal_1_text {
            get {
                return ResourceManager.GetString("event_journal_1_text", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A new PQP archive has been created.
        /// </summary>
        internal static string event_journal_2_text {
            get {
                return ResourceManager.GetString("event_journal_2_text", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The memory was formatted.
        /// </summary>
        internal static string event_journal_3_text {
            get {
                return ResourceManager.GetString("event_journal_3_text", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Program rerun.
        /// </summary>
        internal static string event_journal_4_text {
            get {
                return ResourceManager.GetString("event_journal_4_text", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Information.
        /// </summary>
        internal static string information_caption {
            get {
                return ResourceManager.GetString("information_caption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to CRC errors occured while reading system information on pages: {0}!.
        /// </summary>
        internal static string msg_crc_error_pages {
            get {
                return ResourceManager.GetString("msg_crc_error_pages", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not establish connection with database {0} on host {1}:{2}.
        ///Please check PostgreSQL server and try another time..
        /// </summary>
        internal static string msg_db_connect_error_text {
            get {
                return ResourceManager.GetString("msg_db_connect_error_text", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot establish connection with the Device Em-33T. This service isn&apos;t incarnated for other devices! 
        ///Interface: {0} (interface parameters: {1}{2})..
        /// </summary>
        internal static string msg_device_connect_em33t_error_text {
            get {
                return ResourceManager.GetString("msg_device_connect_em33t_error_text", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Connection error.
        /// </summary>
        internal static string msg_device_connect_error_caption {
            get {
                return ResourceManager.GetString("msg_device_connect_error_caption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot establish connection with the Device.
        ///Interface: {0} (interface parameters: {1}{2})..
        /// </summary>
        internal static string msg_device_connect_error_text {
            get {
                return ResourceManager.GetString("msg_device_connect_error_text", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Reading error.
        /// </summary>
        internal static string msg_device_connect_lost_caption {
            get {
                return ResourceManager.GetString("msg_device_connect_lost_caption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The connection with the Device has been lost.
        ///Interface: {0} (interface parameters: {1}{2}).
        ///Saving process has been interrupted..
        /// </summary>
        internal static string msg_device_connect_lost_text {
            get {
                return ResourceManager.GetString("msg_device_connect_lost_text", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The data reading error has been occurred..
        /// </summary>
        internal static string msg_device_data_reading_error_caption {
            get {
                return ResourceManager.GetString("msg_device_data_reading_error_caption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot read system information from the Device.
        ///Interface: {0} (interface parameters: {1}{2})..
        /// </summary>
        internal static string msg_device_devinfo_error_text {
            get {
                return ResourceManager.GetString("msg_device_devinfo_error_text", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Device&apos;s memory contains no archives.
        /// </summary>
        internal static string msg_device_empty_text {
            get {
                return ResourceManager.GetString("msg_device_empty_text", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The device is not registered.
        /// </summary>
        internal static string msg_device_licence_failed_caption {
            get {
                return ResourceManager.GetString("msg_device_licence_failed_caption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The device is not registered.
        ///If you have the licence file for the device&apos;s identification,
        ///open Settings &lt; Options &lt; Device manager and register it..
        /// </summary>
        internal static string msg_device_licence_failed_text {
            get {
                return ResourceManager.GetString("msg_device_licence_failed_text", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was no event during this period.
        /// </summary>
        internal static string msg_dns_empty_text {
            get {
                return ResourceManager.GetString("msg_dns_empty_text", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The Device {0} doesn&apos;t support {1} I/O interface..
        /// </summary>
        internal static string msg_invalid_interface_text {
            get {
                return ResourceManager.GetString("msg_invalid_interface_text", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to read data for the selected time interval (AVG).
        /// </summary>
        internal static string msg_not_read_avg_index {
            get {
                return ResourceManager.GetString("msg_not_read_avg_index", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Current time of the computer and current time of the device are different! It&apos;s necessary to correct time!.
        /// </summary>
        internal static string msg_time_not_correct {
            get {
                return ResourceManager.GetString("msg_time_not_correct", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Importing data from the Image file.
        /// </summary>
        internal static string name_exchange_window_caption {
            get {
                return ResourceManager.GetString("name_exchange_window_caption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Image file data:.
        /// </summary>
        internal static string name_exchange_window_tree_caption {
            get {
                return ResourceManager.GetString("name_exchange_window_tree_caption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Averaged Network Parameters.
        /// </summary>
        internal static string name_measure_type_avg_full {
            get {
                return ResourceManager.GetString("name_measure_type_avg_full", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Voltage Dips and Swells .
        /// </summary>
        internal static string name_measure_type_dns_full {
            get {
                return ResourceManager.GetString("name_measure_type_dns_full", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Power Quality Parameters.
        /// </summary>
        internal static string name_measure_type_pke_full {
            get {
                return ResourceManager.GetString("name_measure_type_pke_full", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Harmonic powers and angles.
        /// </summary>
        internal static string name_submeasure_avg_angles {
            get {
                return ResourceManager.GetString("name_submeasure_avg_angles", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Individual Harmonic Ratios.
        /// </summary>
        internal static string name_submeasure_avg_harmonics {
            get {
                return ResourceManager.GetString("name_submeasure_avg_harmonics", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Basic Network Parameters.
        /// </summary>
        internal static string name_submeasure_avg_main {
            get {
                return ResourceManager.GetString("name_submeasure_avg_main", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to read the PQP archive at {0}..
        /// </summary>
        internal static string pke_reading_failed {
            get {
                return ResourceManager.GetString("pke_reading_failed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unfortunately.
        /// </summary>
        internal static string unfortunately_caption {
            get {
                return ResourceManager.GetString("unfortunately_caption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Warning.
        /// </summary>
        internal static string warning_caption {
            get {
                return ResourceManager.GetString("warning_caption", resourceCulture);
            }
        }
    }
}
