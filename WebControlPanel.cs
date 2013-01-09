using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using System.Text;

namespace AlarmByZones
{
    public class WebControlPanel : MFToolkit.Net.Web.IHttpHandler
    {
        public void ProcessRequest(MFToolkit.Net.Web.HttpContext context)
        {
            string rawURL_string = string.Empty;
            string menu_Pachube = string.Empty;
            string menu_SuperUser = string.Empty;

            //if (Alarm.ConfigDefault.Data.USE_PACHUBE)
            //{
            //    menu_Pachube = "<li><a href='/pachube' title='Generates Pachube Graphics'>PACHUBE GRAPHICS</a></li>";
            //}

            if (context.Request.RawUrl == "/su")
            {
                menu_SuperUser = "<li><a href='/delete-confirm' title='Deletes last event.'>DELETE LAST EVENT LOG</a></li>";
            }

            string menu_Header =
                "<div>" +
                "<ul>" +
                "<li class=\"current\"><a href=\"/\" title='Home'>HOME</a></li>" +
                "<li><a href='/sdcard' title='Retrieve alarm events stored in SD Card'>SD CARD EVENT LOG</a></li>" +
                menu_Pachube + menu_SuperUser +
                "<li><a href='/diag'  title='Diagnostics'>DIAGNOSTICS</a> </li>" +
                "<li><a href='/about' title='Credits and contributors'>ABOUT</a> </li>" +
                "</ul>" +
                "</div>";
            string fileLink = string.Empty;
            try
            {
                rawURL_string = context.Request.RawUrl.Substring(0, 5);
            }
            catch { }

            try
            {
                if (rawURL_string == "/open")
                {
                    string[] url = context.Request.RawUrl.Split('_');                    
                    fileLink = context.Request.RawUrl.Substring(6, context.Request.RawUrl.Length - 6);                    
                    fileLink = Extension.Replace(fileLink,"/","\\");
                    context.Request.RawUrl = "/open";
                }
            }
            catch { }
           
            switch (context.Request.RawUrl)
            {
                case "/su":
                case "/":
                    AlarmByZones.email.SendEmail("Web Server access", "Home web server access.\nAssemblyInfo: " + System.Reflection.Assembly.GetExecutingAssembly().FullName);
                    Console.DEBUG_ACTIVITY(Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].IPAddress);
                    string HTML_tbHeader = "<table class=\"gridtable\"><tr><th><center>Time</center></th><th><center>Zone/Sensor</center></th><th><center>Description</center></th></tr>";
                    string AlarmStatus = Alarm.Common.Alarm_Info.sbActivity.Length == 0 ? "No Alarms/Sensors " : HTML_tbHeader + Alarm.Common.Alarm_Info.sbActivity.ToString();
                    context.Response.ContentType = "text/html";
                    context.Response.WriteLine("<html><head><title>Control Panel - Home</title>");
                    context.Response.WriteLine("<meta name=\"author\"   content=\"Gilberto Garc�a\"/>");
                    context.Response.WriteLine("<meta name=\"mod-date\" content=\"06/24/2012\"/>");
                    context.Response.WriteLine(AlarmByZones.Css_header);
                    context.Response.WriteLine(AlarmByZones.Table_CSS_Style);
                    context.Response.WriteLine("</head><body>");
                    context.Response.WriteLine("<h1>Alarm Activity - Monitor System #1</h1></br>");
                    context.Response.WriteLine(menu_Header);
                    context.Response.WriteLine("<p>Current Time: " + DateTime.Now + "</p></br>");
                    context.Response.WriteLine("Alarm Status Log: " + AlarmStatus);
                    context.Response.WriteLine("</table>");
                    context.Response.WriteLine("<br/><br/>");
                    context.Response.WriteLine("<div style=\"border:1px solid #CCCCCC;\">");
                    context.Response.WriteLine("<p><span class=\"note\">Copyright &#169; 2012 Gilberto Garc&#237;a</span></p>");
                    context.Response.WriteLine("</div></body></html>");
                    //clear variables
                    HTML_tbHeader = null;
                    AlarmStatus = null;
                    menu_Header = null;
                    break;
                case "/about":
                    string about = Properties.Resources.GetString(Properties.Resources.StringResources.about);
                    context.Response.ContentType = "text/html";
                    context.Response.WriteLine(about);
                    //clear variables
                    about = null;
                    break;
                case "/sdcard":
                    context.Response.ContentType = "text/html";
                    context.Response.WriteLine("<html><head><title>Control Panel - SD Card History Log</title>");
                    context.Response.WriteLine("</head><body>");
                    context.Response.WriteLine("<h1>Alarm Activity - Monitor System #1</h1></br>");
                    context.Response.WriteLine("Current Time: " + DateTime.Now + "</br>");
                    if (AlarmByZones.SdCardEventLogger.IsSDCardAvailable())
                    {
                        context.Response.WriteLine("SD Card detected and found the following files:<br/><br/>");
                        context.Response.WriteLine("<form name=\"sdForm\"");
                        int i = 0;
                        try
                        {
                            foreach (string file in AlarmByZones.SdCardEventLogger.FileList)
                            {
                                context.Response.WriteLine(file + "<br/>");
                                Console.DEBUG_ACTIVITY(i.ToString() + " " + file);
                                i++;
                            }
                            context.Response.WriteLine("</form>");
                        }
                        catch (Exception ex)
                        {
                            Debug.Print(ex.Message);
                        }
                    }
                    else
                    {
                        context.Response.WriteLine(AlarmByZones.SdCardEventLogger.NO_SD_CARD);
                        context.Response.WriteLine("<br/>");
                    }
                    context.Response.WriteLine("<br/>");
                    context.Response.WriteLine("<a href=\"/\">Back to main page...</a>");
                    context.Response.WriteLine("<div style=\"border:1px solid #CCCCCC;\">");
                    context.Response.WriteLine("<p><span class=\"note\">Copyright &#169; 2012 Gilberto Garc&#237;a</span></p>");
                    context.Response.WriteLine("</div></body></html>");
                    break;
                case "/open":
                    System.Collections.ArrayList alOpen = new System.Collections.ArrayList();
                    context.Response.ContentType = "text/html";
                    context.Response.WriteLine("<html><head><title>Control Panel - Open SD Card File</title>");
                    context.Response.WriteLine(AlarmByZones.Css_header);
                    context.Response.WriteLine("</head><body>");
                    context.Response.WriteLine("<h1>Alarm Activity - Monitor System #1</h1></br>");
                    context.Response.WriteLine(menu_Header);
                    context.Response.WriteLine("<p>Current Time: " + DateTime.Now + "</p></br>");
                    if (AlarmByZones.SdCardEventLogger.IsSDCardAvailable())
                    {
                        string rawHref = AlarmByZones.SdCardEventLogger.FileList[AlarmByZones.SdCardEventLogger.FileList.Count - 1].ToString();
                        string[] parseHref = rawHref.Split(new Char[] { '<','>'});
                        string LatestException = parseHref[2];                                                
                        //NOTE: if /open = open latest exception, otherwise open the file link
                        fileLink = fileLink == string.Empty ? LatestException : fileLink;                        
                        AlarmByZones.SdCardEventLogger.openFileContent(fileLink, alOpen);
                        context.Response.WriteLine("File: " + fileLink);
                        context.Response.WriteLine("<br/>");
                        context.Response.WriteLine("File Content: ");
                        context.Response.WriteLine("<br/>");
                        foreach (string content in alOpen)
                        {
                            context.Response.WriteLine(content);
                            context.Response.WriteLine("<br/>");
                        }
                        //clear variables
                        rawHref = null;
                        parseHref = null;
                        LatestException = null;
                    }
                    else
                    {
                        context.Response.WriteLine(AlarmByZones.SdCardEventLogger.NO_SD_CARD);
                    }
                    context.Response.WriteLine("<br/><br/>");
                    context.Response.WriteLine("<a href=\"/\">Back to main page...</a>");
                    context.Response.WriteLine("<div style=\"border:1px solid #CCCCCC;\">");
                    context.Response.WriteLine("<p><span class=\"note\">Copyright &#169; 2012 Gilberto Garc&#237;a</span></p>");
                    context.Response.WriteLine("</div></body></html>");
                    //clear variables
                    alOpen.Clear();
                    alOpen = null;
                    menu_Header = null;
                    break;
                case "/pachube":
                    //System.Collections.ArrayList alPachube = new System.Collections.ArrayList();
                    //Pachube.EmbeddableGraphGenerator.EmbeddableHTML.GenerateHTML(alPachube);
                    //context.Response.ContentType = "text/html";
                    //context.Response.WriteLine("<html><head><title>Control Panel - Pachube Graphics</title>");
                    //context.Response.WriteLine(AlarmByZones.Css_header);
                    //context.Response.WriteLine(AlarmByZones.Table_CSS_Style);
                    //context.Response.WriteLine("</head><body>");
                    //context.Response.WriteLine("<h1>Alarm Activity - Monitor System #1</h1></br>");
                    //context.Response.WriteLine(menu_Header);
                    //context.Response.WriteLine("<p>Current Time: " + DateTime.Now + "</p></br>");
                    //foreach (string content in alPachube)
                    //{
                    //    context.Response.WriteLine(content);
                    //    context.Response.WriteLine("<br/>");
                    //}
                    //context.Response.WriteLine("<a href=\"/\">Back to main page...</a>");
                    //context.Response.WriteLine("<div style=\"border:1px solid #CCCCCC;\">");
                    //context.Response.WriteLine("<p><span class=\"note\">Copyright &#169; 2012 Gilberto Garc&#237;a</span></p>");
                    //context.Response.WriteLine("</div></body></html>");
                    ////clear variables
                    //alPachube.Clear();
                    //alPachube = null;
                    //menu_Header = null;
                    break;
                case "/delete-confirm":
                    if (AlarmByZones.SdCardEventLogger.IsSDCardAvailable())
                    {
                        string rawHref = AlarmByZones.SdCardEventLogger.FileList[AlarmByZones.SdCardEventLogger.FileList.Count - 1].ToString();
                        string[] parseHref = rawHref.Split(new Char[] { '<', '>' });
                        //We do not want to delete Config.ini 
                        //string LastFile = rawHref == Alarm.User_Definitions.Constants.ALARM_CONFIG_FILE_PATH ? rawHref : parseHref[2];
                        string LastFile = rawHref == Alarm.User_Definitions.Constants.ALARM_CONFIG_FILE_PATH || 
                            rawHref == Alarm.User_Definitions.Constants.HTML_RESOURCE_HEADER_STYLE ||
                            rawHref == Alarm.User_Definitions.Constants.HTML_RESOURCE_TABLE_STYLE ? rawHref : parseHref[2];
                        context.Response.ContentType = "text/html";
                        context.Response.WriteLine("<html><head><title>Control Panel - Delete SD Card confirm</title>");
                        context.Response.WriteLine(AlarmByZones.Css_header);
                        context.Response.WriteLine("</head><body>");
                        context.Response.WriteLine("<h1>Alarm Activity - Monitor System #1</h1></br>");
                        context.Response.WriteLine(menu_Header);
                        context.Response.WriteLine("<p>Current Time: " + DateTime.Now + "</p></br>");
                        if (LastFile != Alarm.User_Definitions.Constants.ALARM_CONFIG_FILE_PATH &&
                            LastFile != @"\SD\Logs" && LastFile != @"\SD\Exception" &&
                            LastFile != Alarm.User_Definitions.Constants.HTML_RESOURCE_HEADER_STYLE && 
                            LastFile != Alarm.User_Definitions.Constants.HTML_RESOURCE_TABLE_STYLE)
                        {
                            context.Response.WriteLine("<A HREF=\"/delete-last\" onCLick=\"return confirm('Are you sure you want to delete " +LastFile +" file?')\">Delete " 
                                + LastFile + "</A>");                            
                        }
                        else
                        {
                            context.Response.WriteLine("No files to delete.");
                        }
                        context.Response.WriteLine("<br/><br/>");
                        //clear variables
                        rawHref = null;
                        parseHref = null;
                        LastFile = null;
                    }
                    context.Response.WriteLine("<a href=\"/\">Back to main page...</a>");
                    context.Response.WriteLine("<div style=\"border:1px solid #CCCCCC;\">");
                    context.Response.WriteLine("<p><span class=\"note\">Copyright &#169; 2012 Gilberto Garc&#237;a</span></p>");
                    context.Response.WriteLine("</div></body></html>");
                    menu_Header = null;
                    break;
                case "/delete":
                case "/delete-last":
                    context.Response.ContentType = "text/html";
                    context.Response.WriteLine("<html><head><title>Control Panel - Delete SD Card File</title>");
                    context.Response.WriteLine(AlarmByZones.Css_header);
                    context.Response.WriteLine("</head><body>");
                    context.Response.WriteLine("<h1>Alarm Activity - Monitor System #1</h1></br>");
                    context.Response.WriteLine(menu_Header);
                    context.Response.WriteLine("<p>Current Time: " + DateTime.Now + "</p></br>");
                    if (AlarmByZones.SdCardEventLogger.IsSDCardAvailable())
                    {
                        string rawHref = AlarmByZones.SdCardEventLogger.FileList[AlarmByZones.SdCardEventLogger.FileList.Count - 1].ToString();
                        string[] parseHref = rawHref.Split(new Char[] { '<', '>' });
                        //string LastFile = parseHref[2];
                        string LastFile = rawHref == Alarm.User_Definitions.Constants.ALARM_CONFIG_FILE_PATH || 
                            rawHref == Alarm.User_Definitions.Constants.HTML_RESOURCE_HEADER_STYLE ||
                        rawHref == Alarm.User_Definitions.Constants.HTML_RESOURCE_TABLE_STYLE ? rawHref : parseHref[2];

                        if (LastFile != Alarm.User_Definitions.Constants.ALARM_CONFIG_FILE_PATH && 
                            LastFile != @"\SD\Logs" && LastFile != @"\SD\Exception" &&
                            LastFile != Alarm.User_Definitions.Constants.HTML_RESOURCE_HEADER_STYLE && 
                            LastFile != Alarm.User_Definitions.Constants.HTML_RESOURCE_TABLE_STYLE)
                        {
                            AlarmByZones.SdCardEventLogger.deleteFile(LastFile);

                            context.Response.WriteLine("Deleted File: " + LastFile);                            
                        }
                        else
                        {
                            context.Response.WriteLine("No files to delete.");
                        }
                        context.Response.WriteLine("<br/><br/>");
                        //clear variables
                        rawHref = null;
                        parseHref = null;
                        LastFile = null;
                    }
                    context.Response.WriteLine("<a href=\"/\">Back to main page...</a>");
                    context.Response.WriteLine("<div style=\"border:1px solid #CCCCCC;\">");
                    context.Response.WriteLine("<p><span class=\"note\">Copyright &#169; 2012 Gilberto Garc&#237;a</span></p>");
                    context.Response.WriteLine("</div></body></html>");
                    menu_Header = null;
                    break;
                case "/diag":
                case "/diagnostics":
                    //This is very useful when used with Android App "Overlook Wiz"
                    //---------------------------------------------------------------
                    //Overlook Wiz settings:
                    //Host name or IP address: your host settings.
                    //Host custom name shown on widget: Alarm System
                    //Monitored service: Web Server (HTTP)
                    //Service TCP Port: same as entered in Config.ini [NETDUINO_PLUS_HTTP_PORT], otherwise enter the default port 8080
                    //Website URL to be requested: /diag
                    context.Response.ContentType = "text/html";
                    context.Response.WriteLine("<html><head><title>Control Panel - Diagnostics</title>");
                    context.Response.WriteLine("<meta name=\"author\"   content=\"Gilberto Garc�a\"/>");
                    context.Response.WriteLine("<meta name=\"mod-date\" content=\"07/01/2012\"/>");
                    context.Response.WriteLine("</head><body>");
                    context.Response.WriteLine("<h1>Alarm Activity - Monitor System #1 - Diagnostics</h1>");
                    context.Response.WriteLine("Current Time: " + DateTime.Now + "<br/>");
                    context.Response.WriteLine("<p><font face=\"verdana\" color=\"green\">Alarm System is up and running!</font></p>");
                    context.Response.WriteLine("<b>Power Cycle</b>");
                    context.Response.WriteLine("<lu>");
                    context.Response.WriteLine("<li>Last Time since reset: " + AlarmByZones.LastResetCycle + "</li>");
                    context.Response.WriteLine("</lu>");
                    context.Response.WriteLine("<br/>");
                    context.Response.WriteLine("<b>Memory</b>");
                    context.Response.WriteLine("<lu>");
                    context.Response.WriteLine("<li>Available Memory: " + Debug.GC(true) + "</li>");
                    context.Response.WriteLine(AlarmByZones.SdCardEventLogger.SDCardInfo());
                    context.Response.WriteLine("<br/>");
                    context.Response.WriteLine("<b>AssemblyInfo</b>");
                    context.Response.WriteLine("<li>" + System.Reflection.Assembly.GetExecutingAssembly().FullName + "</li><br/>");
                    context.Response.WriteLine("</lu>");
                    context.Response.WriteLine("<br/><br/>");
                    context.Response.WriteLine("<a href=\"/\">Back to main page...</a>");
                    context.Response.WriteLine("<div style=\"border:1px solid #CCCCCC;\">");
                    context.Response.WriteLine("<p><span class=\"note\">Copyright &#169; 2012 Gilberto Garc&#237;a</span></p>");
                    context.Response.WriteLine("</div></body></html>");
                    break;
                default:
                    context.Response.RaiseError(MFToolkit.Net.Web.HttpStatusCode.NotFound);
                    break;
            }
        }

    }
}
