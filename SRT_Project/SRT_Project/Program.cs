using NetCoreServer;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SRT_Project
{
    internal class Program
    {

        string strDBFile = @"./conf.db";
        SQLiteConnection myConn = new SQLiteConnection();

        static void Main(string[] args)
        {
            // WebSocket server port
            int port = 8084;
            if (args.Length > 0)
                port = int.Parse(args[0]);
            // WebSocket server content path

            string www = string.Empty;


            if (args.Length > 1)
                www = args[1];

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Console.WriteLine("Running on Linux...");
                www = (System.IO.Path.GetDirectoryName("/" + System.Reflection.Assembly.GetExecutingAssembly().CodeBase) + @"/www").Replace(@"file:/", "");

            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Console.WriteLine("Running on Mac...");

            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.WriteLine("Running on Windows...");
                www = (System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase) + @"\www").Replace(@"file:\", "");
            }



            Console.WriteLine($"WebSocket server port: {port}");
            Console.WriteLine($"WebSocket server static content path: {www}");
            Console.WriteLine($"WebSocket server website: http://localhost:{port}");

            Console.WriteLine();

            // Create a new WebSocket server
            var server = new ChatServer(IPAddress.Any, port);

            server.AddStaticContent(www, "/");


            // Start the server
            Console.WriteLine("Server starting...");
            server.Start();
            Console.WriteLine("Done!");

            for (; ; )
            {

            }
        }

        class ChatServer : WsServer
        {
            public ChatServer(IPAddress address, int port) : base(address, port) { }

            protected override TcpSession CreateSession() { return new ChatSession(this); }

            protected override void OnError(SocketError error)
            {
                Console.WriteLine($" WebSocket server caught an error with code {error}");
            }
        }

        class ChatSession : WsSession
        {

            string strDBFile = string.Empty;
            SQLiteConnection myConn = new SQLiteConnection();

            public ChatSession(WsServer server) : base(server) { }

            string strFFMPEGExe = "fef584d5a-dc62-49d4-b5ae-6a46be715e8a";
            string strSRTExe = "sef584d5a-dc62-49d4-b5ae-6a46be715e8a";
            string strDeviceList = string.Empty;
            EncoderConfig myConfig = new EncoderConfig();


            public override void OnWsConnected(HttpRequest request)
            {
                Console.WriteLine($"Chat WebSocket session with Id {Id} connected!");

                // Send invite message
            }

            public override void OnWsDisconnected()
            {
                Console.WriteLine($"Chat WebSocket session with Id {Id} disconnected!");
            }

            public override void OnWsReceived(byte[] buffer, long offset, long size)
            {
                string message = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
                Console.WriteLine("Incoming: " + message);

                // Multicast message to all connected sessions
                //((WsServer)Server).MulticastText(message);

                // If the buffer starts with '!' the disconnect the current session
                if (message == "!")
                {
                    Close(1000);
                }
                else if (message == "reqInputList")
                {
                    ListDecklink();
                }
                else if (message == "startServer")
                {
                    StartServer();
                }
                else if (message == "stopServer")
                {
                    StopServer();
                }
                else if (message == "reAttach")
                {
                    ReAttach();
                }
                else if (message.StartsWith("reqSaveSettings"))
                {
                    SaveSettings(message);
                }
                else if (message.StartsWith("reqConfig"))
                {
                    SendConfig();
                }
            }

            private void SendConfig()
            {
                ReadSettings();

                ((WsServer)Server).MulticastText("respSettings|" + myConfig.strInputType + "é" + myConfig.strSelectedCard + "é" + myConfig.strUDPIP + "é" + myConfig.strUDPPort + "é" + myConfig.strSRTIP + "é" + myConfig.strSRTPort + "é" + myConfig.strSRTLatency + "é" + myConfig.strSRTMode + "é" + myConfig.strSRTPass + "é" + myConfig.strVideoCodec + "é" + myConfig.strVideoBitrate + "é" + myConfig.strAudioCodec + "é" + myConfig.strAudioBitrate);
            }

            private void StopServer()
            {
                killFFMpeg();
                killSRTTransmit();
            }

            private void StartServer()
            {
                StopServer();
                StartSRT_Input();
                StartFFMPEG_Input();
            }

            private void ReAttach()
            {
                Process[] pA = Process.GetProcessesByName("fef584d5a-dc62-49d4-b5ae-6a46be715e8a");

                if (pA.Length > 0)
                {
                    Process buildFFMPEG = pA[0];
                    //buildFFMPEG.StartInfo.UseShellExecute = false;
                    //buildFFMPEG.StartInfo.RedirectStandardOutput = true;
                    //buildFFMPEG.StartInfo.RedirectStandardError = true;
                    //buildFFMPEG.StartInfo.CreateNoWindow = true;
                    buildFFMPEG.ErrorDataReceived += buildFFMPEG_ErrorDataReceived;
                    buildFFMPEG.OutputDataReceived += buildFFMPEG_OutputDataReceived;
                    //buildFFMPEG.EnableRaisingEvents = true;
                    //buildFFMPEG.Start();
                    //processID = build.Id;
                    //buildFFMPEG.BeginOutputReadLine();
                    //buildFFMPEG.BeginErrorReadLine();
                }
            }

            private void SaveSettings(string strMsg)
            {
                Console.WriteLine("Save Settings Received");

                string[] strSplit = strMsg.Split("|".ToCharArray());

                if (strSplit.Length > 0)
                {
                    string[] strParams = strSplit[1].Split("é".ToCharArray());

                    myConfig.strSelectedCard = strParams[0];
                    myConfig.strSRTIP = strParams[1];
                    myConfig.strSRTPort = strParams[2];
                    myConfig.strSRTLatency = strParams[3];
                    myConfig.strSRTMode = strParams[4];
                    myConfig.strVideoCodec = strParams[5];
                    myConfig.strVideoBitrate = strParams[6];
                    myConfig.strAudioCodec = strParams[7];
                    myConfig.strAudioBitrate = strParams[8];
                    myConfig.strInputType = strParams[9];
                    myConfig.strUDPIP = strParams[10];
                    myConfig.strUDPPort = strParams[11];
                    myConfig.strSRTPass = strParams[12];

                    try
                    {


                        strDBFile = @"./conf.db";

                        myConn.ConnectionString = "Data Source=" + strDBFile + "; Pooling=false; FailIfMissing=false;";
                        myConn.Open();
                        SQLiteCommand cmd = myConn.CreateCommand();
                        string strSQL = "UPDATE tblconfig SET inputtype='" + myConfig.strInputType + "', card='" + myConfig.strSelectedCard + "', udpip='" + myConfig.strUDPIP + "', udpport='" + myConfig.strUDPPort + "', srtip='" + myConfig.strSRTIP + "', srtport='" + myConfig.strSRTPort + "', srtlatency='" + myConfig.strSRTLatency + "', srtmode='" + myConfig.strSRTMode + "', srtpass='" + myConfig.strSRTPass + "', vcodec='" + myConfig.strVideoCodec + "', vcodecbitrate='" + myConfig.strVideoBitrate + "', acodec='" + myConfig.strAudioCodec + "', acodecbitrate='" + myConfig.strAudioBitrate + "'"; ;
                        cmd.CommandText = strSQL;
                        cmd.ExecuteNonQuery();
                        myConn.Close();
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine(err.ToString());
                    }
                }
            }


            private void ReadSettings()
            {
                try
                {
                    strDBFile = @"./conf.db";

                    myConn.ConnectionString = "Data Source=" + strDBFile + "; Pooling=false; FailIfMissing=false;";
                    myConn.Open();


                    SQLiteCommand cmd = myConn.CreateCommand();
                    string strSQL = "SELECT * FROM tblconfig";
                    cmd.CommandText = strSQL;
                    SQLiteDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        myConfig.strInputType = reader.GetString(0);
                        myConfig.strSelectedCard = reader.GetString(1);
                        myConfig.strUDPIP = reader.GetString(2);
                        myConfig.strUDPPort = reader.GetString(3);
                        myConfig.strSRTIP = reader.GetString(4);
                        myConfig.strSRTPort = reader.GetString(5);
                        myConfig.strSRTLatency = reader.GetString(6);
                        myConfig.strSRTMode = reader.GetString(7);
                        myConfig.strSRTPass = reader.GetString(8);
                        myConfig.strVideoCodec = reader.GetString(9);
                        myConfig.strVideoBitrate = reader.GetString(10);
                        myConfig.strAudioCodec = reader.GetString(11);
                        myConfig.strAudioBitrate = reader.GetString(12);
                    }

                    myConn.Close();
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.ToString());
                }

            }


            private void ListDecklink()
            {
                strDeviceList = string.Empty;

                Console.WriteLine("Launching ffmpeg to list Decklink Devices");
                Process buildLDecklink = new Process();
                buildLDecklink.StartInfo.Arguments = "-f decklink -list_devices 1 -i dummy -hide_banner";
                //buildLDecklink.StartInfo.Arguments = "-sources decklink ";

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    buildLDecklink.StartInfo.FileName = strFFMPEGExe + ".exe";
                }
                else
                {
                    buildLDecklink.StartInfo.FileName = strFFMPEGExe;
                }

                buildLDecklink.StartInfo.UseShellExecute = false;
                buildLDecklink.StartInfo.RedirectStandardOutput = true;
                buildLDecklink.StartInfo.RedirectStandardError = true;
                buildLDecklink.StartInfo.CreateNoWindow = true;
                buildLDecklink.ErrorDataReceived += BuildLDecklink_ErrorDataReceived;
                buildLDecklink.OutputDataReceived += BuildLDecklink_OutputDataReceived;
                buildLDecklink.EnableRaisingEvents = true;
                buildLDecklink.Start();
                buildLDecklink.BeginOutputReadLine();
                buildLDecklink.BeginErrorReadLine();
            }

            private void BuildLDecklink_OutputDataReceived(object sender, DataReceivedEventArgs e)
            {
                if (e.Data != null)
                {

                }
            }

            private void BuildLDecklink_ErrorDataReceived(object sender, DataReceivedEventArgs e)
            {

                if (e.Data != null && e.Data != "")
                {
                    string strError = e.Data.ToString();
                    //Console.WriteLine("ffmpeg result: " + strError);

                    if (strError.Contains("Could not create DeckLink iterator"))
                    {
                        // No card found
                        Console.WriteLine("No Decklink Found!");
                        //SendTextAsync("respInputList|nop");
                        ((WsServer)Server).MulticastText("respInputList|nop");
                        return;
                    }
                    else if (strError.Contains("dummy"))
                    {
                        return;
                    }
                    else if (strError.Contains("'D"))
                    {
                        // Decklink found, get name
                        string[] strSplit = strError.Split("'".ToCharArray());
                        string strDeviceName = strSplit[1].Replace("'", "").Trim();
                        strDeviceList += strDeviceName + "é";
                    }

                    Console.WriteLine("respInputList|" + strDeviceList);

                    if (strDeviceList.Length > 0)
                    {
                        //SendTextAsync("respInputList|" + strDeviceList.Substring(0, strDeviceList.Length - 1));
                        ((WsServer)Server).MulticastText("respInputList|" + strDeviceList.Substring(0, strDeviceList.Length - 1));

                    }
                    else
                    {
                        //SendTextAsync("respInputList|");
                        ((WsServer)Server).MulticastText("respInputList|");
                    }
                }




            }


            private void StartFFMPEG_Input()
            {

                //string strFormat = "Hi50";
                string strVCodec = "h264_nvenc";
                string strFrameRate = "25";
                string strACodec = "aac";
                string strABitrate = "128";
                string strVBitrate = "10000";

                Process buildFFMPEG = new Process();

                if (myConfig.strVideoCodec == "vcodecH264") strVCodec = "libx264";
                if (myConfig.strVideoCodec == "vcodecH265") strVCodec = "libx265";
                if (myConfig.strVideoCodec == "vcodecMPEG2") strVCodec = "mpeg2video";

                if (myConfig.strAudioCodec == "acodecAAC") strACodec = "aac";
                if (myConfig.strAudioCodec == "acodecMP3") strACodec = "mp3";
                if (myConfig.strAudioCodec == "acodecMPEG2") strACodec = "mp2audio";

                strVBitrate = myConfig.strVideoBitrate;
                strABitrate = myConfig.strAudioBitrate;

                if (myConfig.strSelectedCard == "Decklink Not Found")
                {
                    return;
                }
                else if (myConfig.strInputType == "inputUDP")
                {
                    buildFFMPEG.StartInfo.Arguments = "-re -threads 8 -i \"" + myConfig.strUDPIP + ":" + myConfig.strUDPPort + "\" -c:v  " + strVCodec + " -c:a " + strACodec + " -r " + strFrameRate + " -b:a " + strABitrate + "k -b:v " + strVBitrate + "M -preset ultrafast -intra -crf 20 -async 2 -maxrate " + strVBitrate + "M -bufsize " + strVBitrate + "M -pix_fmt yuv420p -x264opts nal-hrd=cbr:keyint=1 -tune zerolatency -aspect 16:9 -f mpegts \"udp://127.0.0.1:5555?pkt_size=1316\"";

                    Console.WriteLine(buildFFMPEG.StartInfo.Arguments);
                }
                else if (myConfig.strInputType == "inputTest")
                {
                    //buildFFMPEG.StartInfo.Arguments = "-re -f lavfi -i smptebars=size=1920x1080:rate=25 -vf drawtext=fontfile=arial.ttf:text=\"" + System.Environment.MachineName + "\r\n\r\n" + @"srt'\://'" + myConfig.strSRTIP + @"'\:'" + myConfig.strSRTPort + "\":fontcolor=black:fontsize=100:x=(w-text_w)/2:y=(h-text_h)/2:borderw=5:bordercolor=white -c:v  " + strVCodec + " -c:a " + strACodec + " -r " + strFrameRate + " -b:a " + strABitrate + "k -b:v " + strVBitrate + "M -preset ultrafast -intra -crf 23 -x264opts nal-hrd=cbr:keyint=1 -tune zerolatency -r 25 -aspect 16:9 -f mpegts \"udp://127.0.0.1:5555?pkt_size=1316\"";
                    buildFFMPEG.StartInfo.Arguments = "-re -f lavfi -i testsrc=1920x1080:rate=25  -c:v  " + strVCodec + " -c:a " + strACodec + " -r " + strFrameRate + " -b:a " + strABitrate + "k -b:v " + strVBitrate + "M -maxrate " + strVBitrate + "M -bufsize " + strVBitrate + "M -preset ultrafast -intra -crf 23 -x264opts nal-hrd=cbr:keyint=1 -tune zerolatency -aspect 16:9 -f mpegts \"udp://127.0.0.1:5555?pkt_size=1316\"";

                    Console.WriteLine(buildFFMPEG.StartInfo.Arguments);
                }
                else
                {
                    buildFFMPEG.StartInfo.Arguments = "-re -threads 8 -format_code Hi50 -f decklink -i \"" + myConfig.strSelectedCard + "\" -c:v  " + strVCodec + " -c:a " + strACodec + " -r " + strFrameRate + " -b:a " + strABitrate + "k -b:v " + strVBitrate + "M -preset ultrafast -intra -crf 20 -async 2 -maxrate " + strVBitrate + "M -bufsize " + strVBitrate + "M -pix_fmt yuv420p -x264opts nal-hrd=cbr:keyint=1 -tune zerolatency -aspect 16:9 -f mpegts \"udp://127.0.0.1:5555?pkt_size=1316\"";
                    Console.WriteLine(buildFFMPEG.StartInfo.Arguments);
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    buildFFMPEG.StartInfo.FileName = strFFMPEGExe + ".exe";
                }
                else
                {
                    buildFFMPEG.StartInfo.FileName = strFFMPEGExe;
                }

                buildFFMPEG.StartInfo.UseShellExecute = false;
                buildFFMPEG.StartInfo.RedirectStandardOutput = true;
                buildFFMPEG.StartInfo.RedirectStandardError = true;
                buildFFMPEG.StartInfo.CreateNoWindow = true;
                buildFFMPEG.ErrorDataReceived += buildFFMPEG_ErrorDataReceived;
                buildFFMPEG.OutputDataReceived += buildFFMPEG_OutputDataReceived;
                buildFFMPEG.EnableRaisingEvents = true;
                buildFFMPEG.Start();
                //processID = build.Id;
                buildFFMPEG.BeginOutputReadLine();
                buildFFMPEG.BeginErrorReadLine();
            }

            private void buildFFMPEG_OutputDataReceived(object sender, DataReceivedEventArgs e)
            {
                if (e.Data != null)
                {
                    //SendTextAsync("logEncoderMsg|" + e.Data.ToString());
                    ((WsServer)Server).MulticastText("logEncoderMsg|" + e.Data.ToString());
                }
            }

            private void buildFFMPEG_ErrorDataReceived(object sender, DataReceivedEventArgs e)
            {
                if (e.Data != null)
                {
                    //SendTextAsync("logEncoderMsg|" + e.Data.ToString());
                    ((WsServer)Server).MulticastText("logEncoderMsg|" + e.Data.ToString());
                }
            }


            private void StartSRT_Input()
            {
                string srtMode = "listener";

                if (myConfig.strSRTMode == "rbCaller") srtMode = "caller";
                if (myConfig.strSRTMode == "rbListener") srtMode = "listener";
                if (myConfig.strSRTMode == "rbRendezvous") srtMode = "rendezvous";

                Process buildSRT = new Process();


                if (myConfig.strSRTMode == "inputUDP")
                {
                    buildSRT.StartInfo.Arguments = "udp://" + myConfig.strUDPIP + ":" + myConfig.strUDPPort + " srt://" + myConfig.strSRTIP + ":" + myConfig.strSRTPort + "?mode=" + srtMode + "&latency=" + myConfig.strSRTLatency;
                    Console.WriteLine(buildSRT.StartInfo.Arguments);
                }
                else
                {
                    buildSRT.StartInfo.Arguments = "udp://:5555 srt://" + myConfig.strSRTIP + ":" + myConfig.strSRTPort + "?mode=" + srtMode + "&latency=" + myConfig.strSRTLatency;
                    Console.WriteLine(buildSRT.StartInfo.Arguments);
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    buildSRT.StartInfo.FileName = strSRTExe + ".exe";
                }
                else
                {
                    buildSRT.StartInfo.FileName = strSRTExe;
                }

                //txtSRTCommand.Text = buildSRT.StartInfo.FileName + " " + buildSRT.StartInfo.Arguments.ToString();

                buildSRT.StartInfo.UseShellExecute = false;
                buildSRT.StartInfo.RedirectStandardOutput = true;
                buildSRT.StartInfo.RedirectStandardError = true;
                buildSRT.StartInfo.CreateNoWindow = true;
                buildSRT.ErrorDataReceived += buildSRT_ErrorDataReceived;
                buildSRT.OutputDataReceived += buildSRT_OutputDataReceived;
                buildSRT.EnableRaisingEvents = true;
                buildSRT.Start();
                //processID = build.Id;
                buildSRT.BeginOutputReadLine();
                buildSRT.BeginErrorReadLine();



            }

            private void buildSRT_OutputDataReceived(object sender, DataReceivedEventArgs e)
            {
                if (e.Data != null)
                {
                    //SendTextAsync("logSRTMsg|" + e.Data.ToString());
                    ((WsServer)Server).MulticastText("logSRTMsg|" + e.Data.ToString());
                }


            }

            private void buildSRT_ErrorDataReceived(object sender, DataReceivedEventArgs e)
            {
                if (e.Data != null)
                {
                    //SendTextAsync("logSRTMsg|" + e.Data.ToString());
                    ((WsServer)Server).MulticastText("logSRTMsg|" + e.Data.ToString());
                }
            }

            private void killSRTTransmit()
            {
                Console.WriteLine("Killing SRT Process");
                Process[] localByName = Process.GetProcessesByName(strSRTExe);

                foreach (Process p in localByName)
                {
                    Console.WriteLine("Found SRT, killing");
                    p.Kill();
                }
            }

            private void killFFMpeg()
            {
                Console.WriteLine("Killing FFMPEG Process");
                Process[] localByName = Process.GetProcessesByName(strFFMPEGExe);

                foreach (Process p in localByName)
                {
                    Console.WriteLine("Found FFMPEG, killing");
                    p.Kill();
                }
            }

            protected override void OnError(SocketError error)
            {
                Console.WriteLine($" WebSocket session caught an error with code {error}");
            }
        }

        public class EncoderConfig
        {
            public string strSelectedCard;
            public string strInputType;
            public string strUDPIP;
            public string strUDPPort;
            public string strSRTIP;
            public string strSRTPort;
            public string strSRTLatency;
            public string strSRTMode;
            public string strVideoCodec;
            public string strVideoBitrate;
            public string strAudioCodec;
            public string strAudioBitrate;
            public string strSRTPass;
        }
    }
}
