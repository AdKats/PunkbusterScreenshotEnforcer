/* support:
 * https://forum.myrcon.com/showthread.php?5202-PunkBuster-ScreenShot-Enforcer-1-3-2-0
 * 
 * grizzlybeer
 * https://forum.myrcon.com/member.php?13930-grizzlybeer
 * 
 * TODO:
 * ok - description update
 * ok - sync vips
 * ok - BF4 compatibility
 * ok - zero score check -> score below x check
 * 
 * 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

using PRoCon.Core;
using PRoCon.Core.Players;
using PRoCon.Core.Plugin;

namespace PRoConEvents
{
    public class PBSSE : PRoConPluginAPI, IPRoConPluginInterface
    {
        private Dictionary<String, Int32> m_playerssreq = new Dictionary<String, Int32>();	//Keeps track of players and pbss requests
        private Dictionary<String, Int32> m_playersssuc = new Dictionary<String, Int32>();  //keeps track of players and their successfully received pbss
        private List<String> whitelist;
        private enumBoolYesNo syncreservedslots;
        //private enumBoolYesNo excludereservedslots;
        private Dictionary<String, DateTime> playerrecentss;
        //private DateTime recentsshelper;

        private Int32 maxreqs; //max ss requests before action
        private enumBoolYesNo enablekick; //enable action kick
        private String kicktext; //text to be displayed to the kicked player
        private enumBoolYesNo enabletban; //enable action tban
        private String tbantext; //text to be displayed to the tbanned player
        private Int32 tbantime; //tban length
        private enumBoolYesNo enableban; //enable action ban
        private String bantext; //text to be displayed to the banned player   
        private enumBoolYesNo banbyname;
        private enumBoolYesNo banbyeaguid;
        private enumBoolYesNo banbypbguid;
        private enumBoolYesNo enableingamenotify; //enable action notify (std)
        private Int32 ingamenotifytime; //display time
        private List<String> ingameaccountname; //admin ingame username to display notify to
        private Int32 debuglevel; //spam debug output to console (0-5)
        private Int32 viphelper; //now this totally sucks, i know that :-) but it works for now. gonna change it later to serverinfo(time something)
        private Int32 maincheckhelper; //now this totally sucks, i know that :-) but it works for now. gonna change it later to serverinfo(time something)
        private Dictionary<String, String> playerips;
        private enumBoolYesNo excludedoubleip;
        private Int32 excludezeroscore;
        private enumBoolYesNo logtofile;
        private String logfilename;

        private Dictionary<String, CPlayerInfo> m_dicPlayerInfo;

        private DateTime lastupdatecheck = DateTime.Now.AddHours(-4);
        private enumBoolYesNo Check4Update;

        public PBSSE()
        {
            this.playerrecentss = new Dictionary<String, DateTime>();
            //this.recentsshelper = new DateTime();
            this.maxreqs = 4;
            this.enablekick = enumBoolYesNo.No;
            this.kicktext = "%maxreqs% screenshots requested, 0 received.";
            this.enabletban = enumBoolYesNo.Yes;
            this.tbantext = "%maxreqs% screenshots requested, 0 received.";
            this.tbantime = 15;
            this.enableban = enumBoolYesNo.No;
            this.bantext = "%maxreqs% screenshots requested, 0 received.";
            this.banbyname = enumBoolYesNo.No;
            this.banbyeaguid = enumBoolYesNo.Yes;
            this.banbypbguid = enumBoolYesNo.No;
            this.enableingamenotify = enumBoolYesNo.Yes;
            this.ingameaccountname = new List<String>();
            this.ingameaccountname.Add("onegrizzlybeer");
            this.ingamenotifytime = 30;
            this.debuglevel = 1;
            this.whitelist = new List<String>();
            this.syncreservedslots = enumBoolYesNo.Yes;
            //this.excludereservedslots = enumBoolYesNo.Yes;
            this.viphelper = 16;
            this.maincheckhelper = 0;
            this.playerips = new Dictionary<String, String>();
            this.excludedoubleip = enumBoolYesNo.Yes;
            this.excludezeroscore = 500;
            this.logtofile = enumBoolYesNo.No;
            this.logfilename = "Plugins/PBSSELogFile.txt";

            this.m_dicPlayerInfo = new Dictionary<String, CPlayerInfo>();

            Check4Update = enumBoolYesNo.Yes;
        }

        public String GetPluginName()
        {
            return "PBSSE";
        }

        public String GetPluginVersion()
        {
            return "1.4.1.1";
        }

        public String GetPluginAuthor()
        {
            return "onegrizzlybeer";
        }

        public String GetPluginWebsite()
        {
            return "www.phogue.net/forumvb/showthread.php?5202-PBSSE-PunkBuster-ScreenShot-Enforcer-BF3";
        }

        public String GetPluginDescription()
        {
            return @"
<h1>PBSSE - PunkBuster ScreenShot Enforcer</h1>
<p>If you like this plugin, please donate using the button below. Any amount is considered helpful. Thank you :-)<br>
<form action=""https://www.paypal.com/cgi-bin/webscr"" method=""post"" target=""_blank"">
<input type=""hidden"" name=""cmd"" value=""_s-xclick"">
<input type=""hidden"" name=""hosted_button_id"" value=""EMMJVSQAQFXG4"">
<input type=""image"" src=""https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif"" border=""0"" name=""submit"" alt=""PayPal - The safer, easier way to pay online!"">
<img alt="" border=""0"" src=""https://www.paypalobjects.com/de_DE/i/scr/pixel.gif"" width=""1"" height=""1"">
</form></p>
<h2>Description</h2>
<p>This plugin monitors the requested and the successfully received Punkbuster screenshots (automatic and manual).<br>
This is neccessary since the Punkbuster Screen Capture Facility (http://www.evenbalance.com/publications/generic-ad/index.htm#screenshots) as nice as it is, allows all screenshots to fail.<br>
For a coder it is very easy to block Punkbuster screenshots. Therefore I have created this plugin.<br>
Note that Punkbuster Screenshots can also fail other reasons, like the video driver (this rarely happens). Minimized games or games on loading screens usually return successful black screens.</p>
<h4>Since this plugin only monitors, you need to set these values in your pbsv.cfg like this:</h4>
<blockquote>
pb_sv_AutoSs 1 //[0=No, 1=Yes (default=0)]<br>
pb_sv_AutoSsFrom 600 //[Min # of seconds to wait before requesting next ss]<br>
pb_sv_AutoSsTo 1200 //[Max # of seconds to wait before requesting next ss]
</blockquote>
This will ensure that Screenshots are requested automatically.
<h2>Support & Feedback</h2>
<a href=""http://www.phogue.net/forumvb/showthread.php?5202-PBSSE-PunkBuster-ScreenShot-Enforcer-BF3"" target=_blank>http://www.phogue.net/forumvb/showthread.php?5202-PBSSE-PunkBuster-ScreenShot-Enforcer-BF3</a>
<h2>Plugin (recommended) Settings</h2>
<blockquote>
1. Check<br>
How much requests do you want to send until kick/notify: 4<br>
Exclude players that have the same ip from check: Players in the same LAN will have the same ip to the pb server. Punkbuster screenshots may fail for this reason. (Yes)<br>
Exclude players that have less than X score from check: Idle Players. (500)<br>
TempBan/Ban by name: Not recommended. A Player can easily change their name and join again. (No)<br>
TempBan/Ban by EA GUID: Recommended. This ID is linked to the player's EA account. Works best with Metabans (Yes)<br>
TempBan/Ban by PB GUID: Recommended. This ID is linked to the player's cd key (No)<br>
Whitelist: List of players (ingame-names without clantag) not to check<br>
Sync ReservedSlots/ServerVIPs: Automatically syncronize the Whitelist with the ReservedSlots list (Yes)<br><br>

2. Kick<br>
Enable Kick on no PB Screenshots: No<br>
Message to be displayed to the kicked player: %maxreqs% will be replaced with the number of screenshot requests. (%maxreqs% screenshots requested, 0 received)<br><br>

3. TBan<br>
Enable Temp Ban on no PB Screenshots: If temp ban is enabled, no kick will happen (Yes)<br>
Message to be displayed to the temp banned player: %maxreqs% will be replaced with the number of screenshot requests. (%maxreqs% screenshots requested, 0 received)<br>
Length of Temp Ban (min): Length of temp ban in minutes (15)<br><br>

4. Ban<br>
Enable ban on no PB Screenshots: If permanent ban is enabled, no temp ban or kick will happen (No)<br>
Message to be displayed to the banned player: %maxreqs% will be replaced with the number of screenshot requests. (%maxreqs% screenshots requested, 0 received)<br><br>

5. Notify<br>
Enable ingame notification: Yell a message to an admin (or any other player) when a player is kicked/banned by this plugin (Yes)<br>
Ingame username: player to receive the ingame notification via pyell<br>
Time to display (sec): Time to display the ingame notification in seconds (30)<br><br>

6. Debug<br>
Debug Level (0-5): Debug level adjusting how many debug messages you will see in the plugin console log (1)<br>
0 - no messages at all (quiet)<br>
1 - only kicks/bans will be displayed<br>
2 - statistics about requested, successfully received screenshots and resets will be displayed<br>
3 - individual requests and receives will be displayed<br>
4 - adding and removing to the lists of requests and receives will be displayed<br>
5 - just for development and testing<br><br>
Log to file: Log plugin output to a file (PBSSELogFile.txt) in the Plugin/BF3 directory. Not recommended, this file may get very big and slow your procon down. Only use it to log errors and only use it local (No)<br>
Filename/Path: Filename and path of the logfile relative to the Procon executable (Plugins/PBSSELogFile.txt)<br>

7. Automatic Update Check settings<br>
Check for Update?: Automatically check for a plugin update every 3 hours. (Yes)<br>
</blockquote>
<h2>Version History (Changelog):</h2>
<blockquote>
<h4>1.4.1.1</h4>
-Fix: Updated plugin description<br>
-Fix: Changed Include/Exclude VIPs to Sync VIPs<br><br>
<h4>1.4.1.0</h4>
-Fix: Added psay to ingame notification. (no yell in BF4)<br><br>
<h4>1.4.0.0</h4>
-NEW: BF4 Compatibility.<br>
-NEW: Automatic Update Check.<br>
-Fix: You can now set the minimum score for players to be checked (replaces exclude players with 0 score = idle players).<br><br>
<h4>1.3.2.0</h4>
-NEW: Added option to automatically delete Non-ReservedSlots/ServerVIPs from the Whitelist.<br><br>
<h4>1.3.1</h4>
-FIX: Added option to exclude players with 0 score from check (idle players).<br><br>
<h4>1.3</h4>
-UPDATE: You can now download/update PBSSE directly through your procon gui.<br><br>
<h4>1.2.5</h4>
-NEW: Option to add multiple usernames to get notified (5. Notify -> ingame username)<br><br>
<h4>1.2.4</h4>
-FIX: Playernames containing spaces were not identified properly<br><br>
<h4>1.2.3</h4>
-FIX: minor code fixes (everything runs much smoother now :-))<br><br>
<h4>1.2.2</h4>
-NEW: Option to log all debug output to a file (PBSSELogFile.txt) in the Plugins/BF3 directory (use with caution)<br>
-NEW: Filter to drop PBScreenshots request if requested too fast after each other (3 minutes)<br>
-FIX: Whitelist was updating too slow under certain circumstances<br><br>
<h4>1.2.1</h4>
-FIX: Some successfully received screenshots were not counted properly under certain circumstances<br><br>
<h4>1.2</h4>
-NEW: Option to automatically exclude players with the same ip (same LAN) from check<br><br>
<h4>1.1.1</h4>
-FIX: statistics (maybe check routine too) where not showing when they should show. this is now fixed<br><br>
<h4>1.1</h4>
-NEW: Whitelist added (+Option to add ReservedSlots/ServerVIPs)<br>
-FIX: Increased default number of requests to 4<br>
-Minor Fixes<br><br>
<h4>1.0</h4>
-Public release
</blockquote>
<h2>Known Issues</h2>
<blockquote>
<h4>Punkbuster Screenshots</h4>
Like mentioned above Punkbuster screenshots can fail for several reasons (drivers, minimized games (idle players), same ip/port (yes its true, this happens when you get your pc to a friend and you both play at the same time on the same server. the nat/pat settings on your router may help. google it :-)))<br>
Workaround: add them to your whitelist<br><br>
<h4>Whitelist</h4>
If you add a player to your ReservedSlots/ServerVIP List they might not show up in the whitelist setting immediately. However you can be sure they are added and processed. I think this is because Procon GUI does not reload the setting when its not changed in the GUI. Just reopen the Plugins Tab and they should be displayed. Or restart Procon on your PC (NOT the layer server, just the exe on your PC) and they will show up.<br>
Workaround: Not really needed, however if someone knows a solution i would like to know it too :-)<br><br>
<h4>Logfile</h4>
This file may get very big and slow your procon down. Only use it to log errors and only use it local. The directory must also exist and be writeable by your procon.
</blockquote>
";
        }

        public void OnPluginLoaded(String strHostName, String strPort, String strPRoConVersion)
        {
            this.RegisterEvents(this.GetType().Name,
                "OnListPlayers",
                "OnPunkbusterMessage",
                "OnLevelLoaded",
                "OnReservedSlotsList",
                "OnPunkbusterPlayerInfo",
                "OnPlayerLeft",
                "OnServerInfo"
                );
        }

        public void OnPluginEnable()
        {
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bPBSSE ^2Enabled!");
        }

        public void OnPluginDisable()
        {
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bPBSSE ^1Disabled =(");
        }

        public List<CPluginVariable> GetDisplayPluginVariables()
        {
            List<CPluginVariable> lstReturn = new List<CPluginVariable>();

            lstReturn.Add(new CPluginVariable("1. Check|How much requests do you want to send until kick/notify", this.maxreqs.GetType(), this.maxreqs));
            lstReturn.Add(new CPluginVariable("1. Check|Exclude players that have the same ip from check", typeof(enumBoolYesNo), this.excludedoubleip));
            lstReturn.Add(new CPluginVariable("1. Check|Exclude players that have less than X score from check", excludezeroscore.GetType(), this.excludezeroscore));
            lstReturn.Add(new CPluginVariable("1. Check|TempBan/Ban by name", typeof(enumBoolYesNo), this.banbyname));
            lstReturn.Add(new CPluginVariable("1. Check|TempBan/Ban by EA GUID", typeof(enumBoolYesNo), this.banbyeaguid));
            lstReturn.Add(new CPluginVariable("1. Check|TempBan/Ban by PB GUID", typeof(enumBoolYesNo), this.banbypbguid));
            lstReturn.Add(new CPluginVariable("1. Check|Whitelist", typeof(String[]), this.whitelist.ToArray()));
            lstReturn.Add(new CPluginVariable("1. Check|Sync ReservedSlots/ServerVIPs", typeof(enumBoolYesNo), this.syncreservedslots));
            //lstReturn.Add(new CPluginVariable("1. Check|Exclude Non-ReservedSlots/ServerVIPs", typeof(enumBoolYesNo), this.excludereservedslots)); 

            lstReturn.Add(new CPluginVariable("2. Kick|Enable Kick on no PB Screenshots", typeof(enumBoolYesNo), this.enablekick));
            if (this.enablekick == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("2. Kick|Message to be displayed to the kicked player", this.kicktext.GetType(), this.kicktext));
            }

            lstReturn.Add(new CPluginVariable("3. TBan|Enable Temp Ban on no PB Screenshots", typeof(enumBoolYesNo), this.enabletban));
            if (this.enabletban == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("3. TBan|Message to be displayed to the temp banned player", this.tbantext.GetType(), this.tbantext));
                lstReturn.Add(new CPluginVariable("3. TBan|Length of Temp Ban (min)", this.tbantime.GetType(), this.tbantime));
            }

            lstReturn.Add(new CPluginVariable("4. Ban|Enable Ban on no PB Screenshots", typeof(enumBoolYesNo), this.enableban));
            if (this.enableban == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("4. Ban|Message to be displayed to the banned player", this.bantext.GetType(), this.bantext));
            }

            lstReturn.Add(new CPluginVariable("5. Notify|Enable ingame notification", typeof(enumBoolYesNo), this.enableingamenotify));
            if (this.enableingamenotify == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("5. Notify|Ingame username", typeof(String[]), this.ingameaccountname.ToArray()));
                lstReturn.Add(new CPluginVariable("5. Notify|Time to display (sec)", this.ingamenotifytime.GetType(), this.ingamenotifytime));
            }

            lstReturn.Add(new CPluginVariable("6. Debug|Debug Level (0-5)", this.debuglevel.GetType(), this.debuglevel));
            lstReturn.Add(new CPluginVariable("6. Debug|Log to file", typeof(enumBoolYesNo), this.logtofile));
            if (this.logtofile == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("6. Debug|Filename/Path", this.logfilename.GetType(), this.logfilename));
            }

            lstReturn.Add(new CPluginVariable("7. Automatic Update Check settings|Check for Update?", this.Check4Update.GetType(), this.Check4Update));

            return lstReturn;
        }

        public List<CPluginVariable> GetPluginVariables()
        {
            List<CPluginVariable> lstReturn = new List<CPluginVariable>();

            lstReturn.Add(new CPluginVariable("How much requests do you want to send until kick/notify", this.maxreqs.GetType(), this.maxreqs));
            lstReturn.Add(new CPluginVariable("Exclude players that have the same ip from check", typeof(enumBoolYesNo), this.excludedoubleip));
            lstReturn.Add(new CPluginVariable("Exclude players that have less than X score from check", excludezeroscore.GetType(), this.excludezeroscore));
            lstReturn.Add(new CPluginVariable("TempBan/Ban by name", typeof(enumBoolYesNo), this.banbyname));
            lstReturn.Add(new CPluginVariable("TempBan/Ban by EA GUID", typeof(enumBoolYesNo), this.banbyeaguid));
            lstReturn.Add(new CPluginVariable("TempBan/Ban by PB GUID", typeof(enumBoolYesNo), this.banbypbguid));
            lstReturn.Add(new CPluginVariable("Whitelist", typeof(String[]), this.whitelist.ToArray()));
            lstReturn.Add(new CPluginVariable("Sync ReservedSlots/ServerVIPs", typeof(enumBoolYesNo), this.syncreservedslots));
            //lstReturn.Add(new CPluginVariable("Exclude Non-ReservedSlots/ServerVIPs", typeof(enumBoolYesNo), this.excludereservedslots)); 

            lstReturn.Add(new CPluginVariable("Enable Kick on no PB Screenshots", typeof(enumBoolYesNo), this.enablekick));
            lstReturn.Add(new CPluginVariable("Message to be displayed to the kicked player", this.kicktext.GetType(), this.kicktext));

            lstReturn.Add(new CPluginVariable("Enable Temp Ban on no PB Screenshots", typeof(enumBoolYesNo), this.enabletban));
            lstReturn.Add(new CPluginVariable("Message to be displayed to the temp banned player", this.tbantext.GetType(), this.tbantext));
            lstReturn.Add(new CPluginVariable("Length of Temp Ban (min)", this.tbantime.GetType(), this.tbantime));

            lstReturn.Add(new CPluginVariable("Enable Ban on no PB Screenshots", typeof(enumBoolYesNo), this.enableban));
            lstReturn.Add(new CPluginVariable("Message to be displayed to the banned player", this.bantext.GetType(), this.bantext));

            lstReturn.Add(new CPluginVariable("Enable ingame notification", typeof(enumBoolYesNo), this.enableingamenotify));
            lstReturn.Add(new CPluginVariable("Ingame username", typeof(String[]), this.ingameaccountname.ToArray()));
            lstReturn.Add(new CPluginVariable("Time to display (sec)", this.ingamenotifytime.GetType(), this.ingamenotifytime));

            lstReturn.Add(new CPluginVariable("Debug Level (0-5)", this.debuglevel.GetType(), this.debuglevel));
            lstReturn.Add(new CPluginVariable("Log to file", typeof(enumBoolYesNo), this.logtofile));
            lstReturn.Add(new CPluginVariable("Filename/Path", this.logfilename.GetType(), this.logfilename));

            lstReturn.Add(new CPluginVariable("Check for Update?", this.Check4Update.GetType(), this.Check4Update));

            return lstReturn;
        }

        public void SetPluginVariable(String strVariable, String strValue)
        {
            Int32 outvalue;

            if (strVariable.CompareTo("Whitelist") == 0)
            {
                this.whitelist = new List<String>(strValue.Split(new char[] { '|' }));
            }
            else if (strVariable.CompareTo("Exclude players that have the same ip from check") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                this.excludedoubleip = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Exclude players that have less than X score from check") == 0 && Int32.TryParse(strValue, out outvalue))
            {
                this.excludezeroscore = outvalue;
            }
            else if (strVariable.CompareTo("Sync ReservedSlots/ServerVIPs") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                this.syncreservedslots = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                this.ExecuteCommand("procon.protected.send", "reservedSlotsList.list");
            }
            //else if (strVariable.CompareTo("Exclude Non-ReservedSlots/ServerVIPs") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            //{
            //    this.excludereservedslots = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            //    this.ExecuteCommand("procon.protected.send", "reservedSlotsList.list");
            //}
            else if (strVariable.CompareTo("TempBan/Ban by name") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                this.banbyname = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                if (this.banbyname == enumBoolYesNo.Yes)
                {
                    this.banbyeaguid = enumBoolYesNo.No;
                    this.banbypbguid = enumBoolYesNo.No;
                }
            }
            else if (strVariable.CompareTo("TempBan/Ban by EA GUID") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                this.banbyeaguid = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                if (this.banbyeaguid == enumBoolYesNo.Yes)
                {
                    this.banbyname = enumBoolYesNo.No;
                    this.banbypbguid = enumBoolYesNo.No;
                }
            }
            else if (strVariable.CompareTo("TempBan/Ban by PB GUID") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                this.banbypbguid = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                if (this.banbypbguid == enumBoolYesNo.Yes)
                {
                    this.banbyname = enumBoolYesNo.No;
                    this.banbyeaguid = enumBoolYesNo.No;
                }
            }
            else if (strVariable.CompareTo("Enable Kick on no PB Screenshots") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                this.enablekick = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                if (this.enablekick == enumBoolYesNo.Yes)
                {
                    this.enabletban = enumBoolYesNo.No;
                    this.enableban = enumBoolYesNo.No;
                }
            }
            else if (strVariable.CompareTo("Enable Temp Ban on no PB Screenshots") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                this.enabletban = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                if (this.enabletban == enumBoolYesNo.Yes)
                {
                    this.enablekick = enumBoolYesNo.No;
                    this.enableban = enumBoolYesNo.No;
                }
            }
            else if (strVariable.CompareTo("Enable Ban on no PB Screenshots") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                this.enableban = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                if (this.enableban == enumBoolYesNo.Yes)
                {
                    this.enabletban = enumBoolYesNo.No;
                    this.enablekick = enumBoolYesNo.No;
                }
            }
            else if (strVariable.CompareTo("Length of Temp Ban (min)") == 0 && Int32.TryParse(strValue, out outvalue))
            {
                this.tbantime = outvalue;

                if (this.tbantime < 0) //not less than 0
                    this.tbantime = 0;

                if (this.tbantime > 133920)
                    this.tbantime = 133920;
            }
            else if (strVariable.CompareTo("Message to be displayed to the kicked player") == 0)
                this.kicktext = strValue;
            else if (strVariable.CompareTo("Message to be displayed to the temp banned player") == 0)
                this.tbantext = strValue;
            else if (strVariable.CompareTo("Message to be displayed to the banned player") == 0)
                this.bantext = strValue;
            else if (strVariable.CompareTo("How much requests do you want to send until kick/notify") == 0 && Int32.TryParse(strValue, out outvalue))
            {
                this.maxreqs = outvalue;

                if (this.maxreqs < 3) //not less than 2 requests                
                    this.maxreqs = 3;

                if (this.maxreqs > 10) //not more than 10 requests (this would take at least 30 minutes)                
                    this.maxreqs = 10;
            }
            else if (strVariable.CompareTo("Enable ingame notification") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
                this.enableingamenotify = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            else if (strVariable.CompareTo("Ingame username") == 0)
                this.ingameaccountname = new List<String>(strValue.Split(new char[] { '|' }));
            else if (strVariable.CompareTo("Time to display (sec)") == 0 && Int32.TryParse(strValue, out outvalue))
            {
                this.ingamenotifytime = outvalue;
                if (this.ingamenotifytime < 1)
                    this.ingamenotifytime = 1;

                if (this.ingamenotifytime > 120) //120 secs is enough                
                    this.ingamenotifytime = 120;
            }
            else if (strVariable.CompareTo("Debug Level (0-5)") == 0 && Int32.TryParse(strValue, out outvalue))
            {
                this.debuglevel = outvalue;
                if (this.debuglevel < 0)
                    this.debuglevel = 0;

                if (this.debuglevel > 5)
                    this.debuglevel = 5;
            }
            else if (strVariable.CompareTo("Log to file") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
                this.logtofile = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            else if (strVariable.CompareTo("Filename/Path") == 0)
                this.logfilename = strValue;
            else if (strVariable.CompareTo("Check for Update?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.Check4Update = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
        }

        public virtual void OnPlayerLeft(CPlayerInfo playerInfo)
        {
            if (this.m_playerssreq.ContainsKey(playerInfo.SoldierName))
            {
                log("PBSSE removing player from requests list: " + playerInfo.SoldierName, 4);
                this.m_playerssreq.Remove(playerInfo.SoldierName);
            }
            if (this.m_playersssuc.ContainsKey(playerInfo.SoldierName))
            {
                log("PBSSE removing player from successfully received list: " + playerInfo.SoldierName, 4);
                this.m_playersssuc.Remove(playerInfo.SoldierName);
            }
            if (this.playerips.ContainsKey(playerInfo.SoldierName))
            {
                log("PBSSE removing player from ip list: " + playerInfo.SoldierName, 4);
                this.playerips.Remove(playerInfo.SoldierName);
            }
            if (this.playerrecentss.ContainsKey(playerInfo.SoldierName))
            {
                log("PBSSE removing player from recentss list: " + playerInfo.SoldierName, 4);
                this.playerrecentss.Remove(playerInfo.SoldierName);
            }
        }

        private void UpdateCheck()
        {
            if (Check4Update == enumBoolYesNo.Yes)
            {
                try
                {
                    DateTime updatehelper = lastupdatecheck.AddHours(3);
                    if (DateTime.Compare(updatehelper, DateTime.Now) <= 0)
                    {
                        WebClient wc = new WebClient();
                        String latestversion = wc.DownloadString("https://forum.myrcon.com/showthread.php?5202");

                        latestversion = latestversion.Substring(latestversion.IndexOf("<title>") + 7);
                        latestversion = latestversion.Substring(0, latestversion.IndexOf("</title>"));
                        latestversion = latestversion.Substring(latestversion.IndexOf("Enforcer") + 9);

                        if (GetPluginVersion() != latestversion)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "PBSSE: ^b^2UPDATE " + latestversion + " AVAILABLE");
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "PBSSE: your version: " + GetPluginVersion());
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "PBSSE: latest version " + latestversion);
                        }
                        lastupdatecheck = DateTime.Now;
                    }
                }
                catch (Exception ex)
                {
                    log("[PBSSE] ERROR checking for Update: " + ex, 1);
                }
            }
        }

        public override void OnServerInfo(CServerInfo csiServerInfo)
        {
            UpdateCheck();
        }

        public override void OnListPlayers(List<CPlayerInfo> lstPlayers, CPlayerSubset cpsSubset)
        {
            log("PBSSE OnListPlayers event", 5);

            //this.recentsshelper = DateTime.Now;

            if (this.maincheckhelper >= 150)
                this.maincheckhelper = 0;
            this.maincheckhelper++;

            if (this.viphelper >= 3)
            {
                if (this.syncreservedslots == enumBoolYesNo.Yes)
                {
                    log("PBSSE requesting ReservedSlots", 4);
                    this.ExecuteCommand("procon.protected.send", "reservedSlotsList.list");
                }
                this.viphelper = 0;
            }
            this.viphelper++;

            foreach (CPlayerInfo cpiPlayer in lstPlayers)
            {
                log("PBSSE checking playerdict: " + cpiPlayer.SoldierName, 5);
                if (this.m_dicPlayerInfo.ContainsKey(cpiPlayer.SoldierName))
                {
                    this.m_dicPlayerInfo[cpiPlayer.SoldierName] = cpiPlayer;
                }
                else
                {
                    log("PBSSE playerdict adding: " + cpiPlayer.SoldierName, 4);
                    this.m_dicPlayerInfo.Add(cpiPlayer.SoldierName, cpiPlayer);
                }
            }

            //foreach (KeyValuePair<string, int> kvp in this.m_playerssreq) //housekeeping the lists: players in our list that do not exist, requests
            //{
            //    bool boolplayerexists = false;
            //    foreach (CPlayerInfo cpiPlayer in lstPlayers)
            //    {
            //        if (cpiPlayer.SoldierName == kvp.Key)
            //        {
            //            boolplayerexists = true;
            //        }
            //    }
            //    if (!boolplayerexists)
            //    {
            //        log("PBSSE removing player from requests list: " + kvp.Key, 4);                                        
            //        this.m_playerssreq.Remove(kvp.Key);
            //    }
            //}

            //foreach (KeyValuePair<string, int> kvp in this.m_playersssuc) //housekeeping the lists: players in our list that do not exist, successes
            //{
            //    bool boolplayerexists = false;
            //    foreach (CPlayerInfo cpiPlayer in lstPlayers)
            //    {
            //        if (cpiPlayer.SoldierName == kvp.Key)
            //        {
            //            boolplayerexists = true;
            //        }
            //    }
            //    if (!boolplayerexists)
            //    {
            //        log("PBSSE removing player from successfully received list: " + kvp.Key, 4);                      
            //        this.m_playersssuc.Remove(kvp.Key);
            //    }
            //}

            foreach (CPlayerInfo cpiPlayer in lstPlayers) //housekeeping the lists: players that exist but not in our list, requests
            {
                Boolean boolplayerexists = false;
                foreach (KeyValuePair<String, Int32> kvp in this.m_playerssreq)
                {
                    if (cpiPlayer.SoldierName == kvp.Key)
                    {
                        boolplayerexists = true;
                    }
                }
                if (!boolplayerexists)
                {
                    log("PBSSE adding player to requests list: " + cpiPlayer.SoldierName, 4);
                    this.m_playerssreq.Add(cpiPlayer.SoldierName, 0);
                }
            }

            foreach (CPlayerInfo cpiPlayer in lstPlayers) //housekeeping the lists: players that exist but not in our list, successes
            {
                Boolean boolplayerexists = false;
                foreach (KeyValuePair<String, Int32> kvp in this.m_playersssuc)
                {
                    if (cpiPlayer.SoldierName == kvp.Key)
                    {
                        boolplayerexists = true;
                    }
                }
                if (!boolplayerexists)
                {
                    log("PBSSE adding player to successfully received list: " + cpiPlayer.SoldierName, 4);
                    this.m_playersssuc.Add(cpiPlayer.SoldierName, 0);
                }
            }

            //foreach (KeyValuePair<string, string> kvp in this.playerips) //housekeeping the lists: players in our list that do not exist, ips
            //{
            //    bool boolplayerexists = false;
            //    foreach (CPlayerInfo cpiPlayer in lstPlayers)
            //    {
            //        if (cpiPlayer.SoldierName == kvp.Key)
            //        {
            //            boolplayerexists = true;
            //        }
            //    }
            //    if (!boolplayerexists)
            //    {
            //        log("PBSSE removing player from ip list: " + kvp.Key, 4);
            //        this.playerips.Remove(kvp.Key);
            //    }
            //}

            foreach (CPlayerInfo cpiPlayer in lstPlayers) //housekeeping the lists: players that exist but not in our list, screenshots
            {
                Boolean boolplayerexists = false;
                foreach (KeyValuePair<String, DateTime> kvp in this.playerrecentss)
                {
                    if (cpiPlayer.SoldierName == kvp.Key)
                    {
                        boolplayerexists = true;
                    }
                }
                if (!boolplayerexists)
                {
                    log("PBSSE adding player to recentss list: " + cpiPlayer.SoldierName, 4);
                    this.playerrecentss.Add(cpiPlayer.SoldierName, DateTime.Now.Add(new TimeSpan(0, -4, 0)));
                }
            }

            //foreach (KeyValuePair<string, DateTime> kvp in this.playerrecentss) //housekeeping the lists: players in our list that do not exist, screenshots
            //{
            //    bool boolplayerexists = false;
            //    foreach (CPlayerInfo cpiPlayer in lstPlayers)
            //    {
            //        if (cpiPlayer.SoldierName == kvp.Key)
            //        {
            //            boolplayerexists = true;
            //        }
            //    }
            //    if (!boolplayerexists)
            //    {
            //        log("PBSSE removing player from recentss list: " + kvp.Key, 4);
            //        this.playerrecentss.Remove(kvp.Key);
            //    }
            //}
        }

        public override void OnPunkbusterMessage(String punkbusterMessage) //main filter routine: check each pb message for certain keywords
        {
            if (punkbusterMessage.IndexOf("screenshot", StringComparison.CurrentCultureIgnoreCase) > -1) //does the word screenshot exist in the msg
            {
                if (punkbusterMessage.IndexOf("successfully received", StringComparison.CurrentCultureIgnoreCase) > -1) //screenshot successfully received
                {
                    String blub; //helper
                    //blub = punkbusterMessage.Substring(154); //cutoff stuff we dont need, like path of screenshot, md5,...
                    blub = punkbusterMessage.Substring(punkbusterMessage.IndexOf("from") + 6); //should be better than above
                    if (blub.IndexOf(" ") > 0) //helper to check if the playernumber is 1 or 2 digits (makes a difference here)
                    {
                        blub = blub.Substring(2);
                    }
                    else
                    {
                        blub = blub.Substring(1);
                    }
                    //blub = blub.Substring(0, blub.IndexOf(" ")); //cut off the remaining junk. now: blub = playername...or not...
                    blub = blub.Substring(0, blub.IndexOf(" ["));

                    log("PBSSE PBScreenshot successfully received: " + blub, 3);
                    if (!this.m_playerssreq.ContainsKey(blub)) //housekeeping the lists
                    {
                        log("PBSSE adding player to requests list: " + blub, 4);
                        this.m_playerssreq.Add(blub, 0);
                    }
                    if (!this.m_playersssuc.ContainsKey(blub))
                    {
                        log("PBSSE adding player to successfully received list: " + blub, 4);
                        this.m_playersssuc.Add(blub, 0);
                    }
                    this.m_playersssuc[blub]++; //increase successful count by 1                    
                }
                else if (punkbusterMessage.IndexOf("requested", StringComparison.CurrentCultureIgnoreCase) > -1) //screenshot requested msg, both auto and manual
                {
                    if (punkbusterMessage.Length > 60)
                    {
                        String blub; //helper
                        blub = punkbusterMessage.Substring(punkbusterMessage.IndexOf("from") + 8); //lets find the first "from". after this auto and manual screenshot requests are the same. 1 and 2 digit playernumbers are the same here.
                        blub = blub.TrimEnd('\r', '\n'); //stupid carriage returns messing up my logs...now: blub=playername

                        if (!this.m_playerssreq.ContainsKey(blub)) //housekeeping the lists
                        {
                            log("PBSSE adding player to requests list: " + blub, 4);
                            this.m_playerssreq.Add(blub, 0);
                        }
                        if (!this.m_playersssuc.ContainsKey(blub)) //housekeeping the lists
                        {
                            log("PBSSE adding player to successfully received list: " + blub, 4);
                            this.m_playersssuc.Add(blub, 0);
                        }
                        if (!this.playerrecentss.ContainsKey(blub)) //housekeeping the lists
                        {
                            log("PBSSE adding player to recentss list: " + blub, 4);
                            this.playerrecentss.Add(blub, DateTime.Now.Add(new TimeSpan(0, -4, 0)));
                        }

                        DateTime freefornewss = this.playerrecentss[blub];
                        freefornewss = freefornewss.AddMinutes(3.0);
                        if (DateTime.Compare(freefornewss, DateTime.Now) >= 0)
                        {
                            log("PBSSE less than 3 minutes since the last PBSS request, dropping request: " + blub, 3);
                        }
                        else
                        {
                            log("PBSSE PBScreenshot requested: " + blub, 3);
                            this.m_playerssreq[blub]++;
                            this.playerrecentss[blub] = DateTime.Now;
                        }
                    }
                }

                //main check routine, this is where the magic *lol* happens :-)
                if (this.maincheckhelper >= 10)
                {
                    log("PBSSE ------------------------", 2);
                    List<String> playerstoreset = new List<String>();
                    foreach (KeyValuePair<String, Int32> kvp in this.m_playerssreq) //take each player in our list (which should be accurate)
                    {
                        //if (this.m_playersssuc[kvp.Key] > kvp.Value) //you cant have more successes than requests
                        //{
                        //    this.m_playersssuc[kvp.Key] = kvp.Value;
                        //}

                        log("PBSSE requests: " + kvp.Value + " successfully received: " + this.m_playersssuc[kvp.Key] + " " + kvp.Key, 2);
                        if (kvp.Value >= this.maxreqs) //did we request enough?
                        {
                            Boolean doubleip = false;
                            if (this.excludedoubleip == enumBoolYesNo.Yes)
                            {
                                foreach (KeyValuePair<String, String> player2 in this.playerips)
                                {
                                    log("PBSSE check ip: " + kvp.Key + ": " + this.playerips[kvp.Key] + " " + player2.Key + ": " + player2.Value, 5);
                                    if (this.playerips[kvp.Key] == player2.Value && kvp.Key != player2.Key)
                                    {
                                        doubleip = true;
                                        log("PBSSE same ip: " + kvp.Key + ", " + player2.Key, 4);
                                    }
                                }
                            }

                            Boolean zeroscore = false;
                            if (this.m_dicPlayerInfo[kvp.Key].Score <= excludezeroscore)
                            {
                                zeroscore = true;
                                log("PBSSE 0 Score detected, skipping player: " + kvp.Key, 2);
                            }

                            if (doubleip) //debuginfo                
                                log("PBSSE found same ip, skipping player: " + kvp.Key, 2);

                            if (this.whitelist.Contains(kvp.Key)) //debuginfo                
                                log("PBSSE found whitelisted player, skipping player: " + kvp.Key, 2);

                            //if (this.debuglevel >= 1 && ((this.m_playersssuc.Count - this.m_playerssreq.Count) > 3 || (this.m_playersssuc.Count - this.m_playerssreq.Count) < -3))
                            //    this.ExecuteCommand("procon.protected.pluginconsole.write", "PBSSE self-check failed: " + this.m_playerssreq.Count + " != " + this.m_playersssuc.Count);

                            if (/*this.m_playersssuc.ContainsKey(kvp.Key) && */this.m_playersssuc[kvp.Key] == 0 && !this.whitelist.Contains(kvp.Key) && !doubleip && !zeroscore/* && ((this.m_playersssuc.Count - this.m_playerssreq.Count) <= 3 || (this.m_playersssuc.Count - this.m_playerssreq.Count) >= -3)*/) //did we get 0 successful screenshots? NOT whitelisted? NOT double ip? NOT 0 score?
                            {
                                if (this.enableban == enumBoolYesNo.Yes) //is ban enabled?
                                {
                                    String msg = this.bantext;
                                    msg = msg.Replace("%maxreqs%", this.maxreqs.ToString());
                                    if (this.banbyname == enumBoolYesNo.Yes || this.banbyeaguid == enumBoolYesNo.Yes)
                                    {
                                        if (this.banbyeaguid == enumBoolYesNo.Yes)
                                            this.ExecuteCommand("procon.protected.send", "banList.add", "guid", this.m_dicPlayerInfo[kvp.Key].GUID, "perm", kvp.Key + " " + msg);
                                        if (this.banbyname == enumBoolYesNo.Yes)
                                            this.ExecuteCommand("procon.protected.send", "banList.add", "name", kvp.Key, "perm", kvp.Key + " " + msg);
                                        this.ExecuteCommand("procon.protected.send", "banList.save");
                                        this.ExecuteCommand("procon.protected.send", "banList.list");
                                    }
                                    if (this.banbypbguid == enumBoolYesNo.Yes)
                                    {
                                        this.ExecuteCommand("procon.protected.send", "punkBuster.pb_sv_command", String.Format("pb_sv_ban \"{0}\" \"{1}\"", kvp.Key, "BC2! " + msg));
                                    }
                                }
                                else if (this.enabletban == enumBoolYesNo.Yes) //is tban enabled?
                                {
                                    String msg = this.tbantext;
                                    msg = msg.Replace("%maxreqs%", this.maxreqs.ToString());
                                    if (this.banbyname == enumBoolYesNo.Yes || this.banbyeaguid == enumBoolYesNo.Yes)
                                    {
                                        if (this.banbyeaguid == enumBoolYesNo.Yes)
                                            this.ExecuteCommand("procon.protected.send", "banList.add", "guid", this.m_dicPlayerInfo[kvp.Key].GUID, "seconds", (this.tbantime * 60).ToString(), kvp.Key + " " + msg);
                                        if (this.banbyname == enumBoolYesNo.Yes)
                                            this.ExecuteCommand("procon.protected.send", "banList.add", "name", kvp.Key, "seconds", (this.tbantime * 60).ToString(), kvp.Key + " " + msg);
                                        this.ExecuteCommand("procon.protected.send", "banList.save");
                                        this.ExecuteCommand("procon.protected.send", "banList.list");
                                    }
                                    if (this.banbypbguid == enumBoolYesNo.Yes)
                                    {
                                        this.ExecuteCommand("procon.protected.send", "punkBuster.pb_sv_command", String.Format("pb_sv_kick \"{0}\" {1} \"{2}\"", kvp.Key, this.tbantime.ToString(), "BC2! " + msg));
                                    }
                                }
                                else if (this.enablekick == enumBoolYesNo.Yes) //is kick enabled?
                                {
                                    String msg = this.kicktext;
                                    msg = msg.Replace("%maxreqs%", this.maxreqs.ToString());
                                    this.ExecuteCommand("procon.protected.send", "admin.kickPlayer", kvp.Key, msg);
                                }

                                if (this.enableingamenotify == enumBoolYesNo.Yes) //is ingamenotify enabled?                            
                                {
                                    foreach (String name in this.ingameaccountname)
                                    {
                                        this.ExecuteCommand("procon.protected.tasks.add", "PBSSE", "0", "1", "1", "procon.protected.send", "admin.yell", "PBSSE RED FLAG: " + kvp.Key + " requests: " + kvp.Value + " successfully received: 0", this.ingamenotifytime.ToString(), "player", name); //works
                                        this.ExecuteCommand("procon.protected.tasks.add", "PBSSE", "0", "1", "1", "procon.protected.send", "admin.say", "PBSSE RED FLAG: " + kvp.Key + " requests: " + kvp.Value + " successfully received: 0", "player", name); //works?
                                    }
                                }
                                log("PBSSE RED FLAG: " + kvp.Key + " requests: " + kvp.Value + " successfully received: 0", 1); //print red flag to plugin console                            
                            }
                            //this.m_playerssreq[kvp.Key] = 0; //reset because we reached maxrequests
                            //this.m_playersssuc[kvp.Key] = 0;
                            playerstoreset.Add(kvp.Key);
                        }
                    }
                    foreach (String player in playerstoreset)
                    {
                        log("PBSSE resetting requests and received: " + player, 2);
                        this.m_playerssreq[player] = 0;
                        this.m_playersssuc[player] = 0;
                    }
                    this.maincheckhelper = 0;
                    log("PBSSE ------------------------", 2);
                }
                this.maincheckhelper++;

                if (this.viphelper >= 15)
                {
                    if (this.syncreservedslots == enumBoolYesNo.Yes)
                    {
                        log("PBSSE requesting ReservedSlots", 4);
                        this.ExecuteCommand("procon.protected.send", "reservedSlotsList.list");
                    }
                    this.viphelper = 0;
                }
                this.viphelper++;
            }
        }

        public override void OnPunkbusterPlayerInfo(CPunkbusterInfo playerInfo)
        {
            log("PBSSE checking ip: " + playerInfo.Ip + " for player: " + playerInfo.SoldierName, 5);
            if (!this.playerips.ContainsKey(playerInfo.SoldierName))
            {
                log("PBSSE adding ip: " + playerInfo.Ip + " for player: " + playerInfo.SoldierName, 4);
                this.playerips.Add(playerInfo.SoldierName, playerInfo.Ip);
            }
        }

        public override void OnLevelLoaded(String mapFileName, String Gamemode, Int32 roundsPlayed, Int32 roundsTotal) //lets make a reset every time a level is loaded (every new round)
        {
            log("PBSSE resetting...", 2);

            this.m_playerssreq = new Dictionary<String, Int32>();	// Keeps track of players and pbss requests
            this.m_playersssuc = new Dictionary<String, Int32>();

            this.m_dicPlayerInfo = new Dictionary<String, CPlayerInfo>();
            this.playerips = new Dictionary<String, String>();
            this.playerrecentss = new Dictionary<String, DateTime>();
        }

        public override void OnReservedSlotsList(List<String> soldierNames)
        {
            if (this.syncreservedslots == enumBoolYesNo.Yes)
            {
                foreach (String vipplayer in soldierNames)
                {
                    if (!this.whitelist.Contains(vipplayer))
                    {
                        this.whitelist.Add(vipplayer);
                    }
                }
                //}

                //if (this.excludereservedslots == enumBoolYesNo.Yes)
                //{
                List<String> playerstoremove = new List<String>();
                foreach (String vipplayer in whitelist)
                {
                    if (!soldierNames.Contains(vipplayer))
                    {
                        playerstoremove.Add(vipplayer);
                    }
                }
                foreach (String player in playerstoremove)
                {
                    whitelist.Remove(player);
                }
            }
        }

        private void log(String msg, Int32 debuglvl)
        {
            if (this.debuglevel >= debuglvl)
            {
                this.ExecuteCommand("procon.protected.pluginconsole.write", msg);

                if (this.logtofile == enumBoolYesNo.Yes)
                {
                    try
                    {
                        String file = Path.Combine(Directory.GetParent(Application.ExecutablePath).FullName, this.logfilename);

                        if (!File.Exists(file))
                            File.Create(file);

                        using (FileStream fs = File.Open(file, FileMode.Append))
                        {
                            Byte[] bytes = new UTF8Encoding(true).GetBytes(DateTime.Now.ToString() + ": " + msg + Environment.NewLine);
                            fs.Write(bytes, 0, bytes.Length);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.ExecuteCommand("unable to append data to file " + ex.Message);
                    }
                }
            }
        }
    }
}
