using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SRT_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class HomeController : ControllerBase
    {
        string strFFMPEGExe = "fef584d5a-dc62-49d4-b5ae-6a46be715e8a";
        string strSRTExe = "sef584d5a-dc62-49d4-b5ae-6a46be715e8a";
        EncoderConfig myConfig = new EncoderConfig();

        string strDBFile = @"./conf.db";
        SqliteConnection myConn = new SqliteConnection();

        // GET api/home
        //[HttpGet]
        //public ActionResult<string> Get()
        //{
        //    using var process = new Process
        //    {
        //        StartInfo =
        //        {
        //            FileName = @"..\SRT_API\bin\Debug\net6.0\ffmpeg.exe",
        //            Arguments = "-re -threads 8 -i \"udp://@224.1.3.1:1000\" -c:v  libx264 -c:a aac -r 25 -b:a 96k -b:v 20M -preset ultrafast -intra -crf 20 -async 2 -maxrate 20M -bufsize 20M -pix_fmt yuv420p -x264opts nal-hrd=cbr:keyint=1 -tune zerolatency -aspect 16:9 -f mpegts \"udp://127.0.0.1:5555?pkt_size=1316\"",
        //            CreateNoWindow = true,
        //            UseShellExecute = false,
        //            RedirectStandardOutput = true,
        //            RedirectStandardError = true
        //        }
        //    };

        //    process.OutputDataReceived += (_, data) => Console.WriteLine(data.Data);
        //    process.ErrorDataReceived += (_, data) => Console.WriteLine(data.Data);
        //    Console.WriteLine("starting");
        //    process.Start();
        //    process.BeginOutputReadLine();
        //    process.BeginErrorReadLine();

        //    //var exited = process.WaitForExit(1000 * 10);     // (optional) wait up to 10 seconds
        //    //Console.WriteLine($"exit {exited}");

        //    return "value";
        //}

                
        [HttpGet("{udpIp}/{udpPort}/{srtIp}/{srtPort}/{latency}/{strVideoCodec}/{strVideoBitrate}/{strAudioCodec}/{strAudioBitrate}/{strSRTMode}")]
        public ActionResult<string> Get(string udpIp, string udpPort, string srtIp, string srtPort, string latency, string strVideoCodec, string strVideoBitrate, string strAudioCodec, string strAudioBitrate, string strSRTMode)
        {

            StopServer();
            StartSRT_Input(srtIp, srtPort, latency, strSRTMode);
            StartFFMPEG_Input(udpIp, udpPort, strVideoCodec, strVideoBitrate, strAudioCodec, strAudioBitrate);

            return "value";

            //{
            //    //return RedirectToAction("Completed", "SRT");

            //    //using var process = new Process
            //    //{
            //    //    StartInfo =
            //    //    {
            //    //        FileName = @"..\SRT_API\bin\Debug\net6.0\ffmpeg.exe",
            //    //        Arguments = "-re -threads 8 -i "udp://@224.30.30.10:1000" -c:v  libx264 -c:a aac -r 25 -b:a 96k -b:v 20M -preset ultrafast  -crf 20 -async 2 -maxrate 20M -bufsize 20M -pix_fmt yuv420p -x264opts nal-hrd=cbr:keyint=1 -tune zerolatency -aspect 16:9 -f mpegts "udp://127.0.0.1:5555?pkt_size=1316"",
            //    //        CreateNoWindow = true,
            //    //        UseShellExecute = false,
            //    //        RedirectStandardOutput = true,
            //    //        RedirectStandardError = true
            //    //    }
            //    //udp://@224.30.30.10
            //    //};

            //    //process.OutputDataReceived += (_, data) => Console.WriteLine(data.Data);
            //    //process.ErrorDataReceived += (_, data) => Console.WriteLine(data.Data);
            //    //Console.WriteLine("starting");
            //    //process.Start();
            //    //process.BeginOutputReadLine();
            //    //process.BeginErrorReadLine();

            //    //var exited = process.WaitForExit(1000 * 10);     // (optional) wait up to 10 seconds
            //    //Console.WriteLine($"exit {exited}");
            //}


        }

        private void StopServer()
        {
            killFFMpeg();
            killSRTTransmit();
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

        private void StartSRT_Input(string srtIp, string srtPort, string latency, string strSRTMode)
        {
            //if (myConfig.strSRTMode == "rbCaller") srtMode = "caller";
            //if (myConfig.strSRTMode == "rbListener") srtMode = "listener";
            //if (myConfig.strSRTMode == "rbRendezvous") srtMode = "rendezvous";            

            Process buildSRT = new Process();

            //if (myConfig.strSRTMode == "inputUDP")
            //{
            //buildSRT.StartInfo.Arguments = "udp://@" + udpIp + ":" + udpPort + " srt://" + srtIp + ":" + srtPort + "?mode=" + srtMode + "&latency=" + latency;
            //Console.WriteLine(buildSRT.StartInfo.Arguments);
            //}
            //else
            //{
            buildSRT.StartInfo.Arguments = "udp://:5555 srt://" + srtIp + ":" + srtPort + "?mode=" + strSRTMode + "&latency=" + latency;
            Console.WriteLine(buildSRT.StartInfo.Arguments);
            //}

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
            //buildSRT.ErrorDataReceived += buildSRT_ErrorDataReceived;
            //buildSRT.OutputDataReceived += buildSRT_OutputDataReceived;
            buildSRT.EnableRaisingEvents = true;
            buildSRT.Start();
            //processID = build.Id;
            buildSRT.BeginOutputReadLine();
            buildSRT.BeginErrorReadLine();
        }


        //private void StartSRT_Input()
        //{
        //    string srtMode = "listener";

        //    if (myConfig.strSRTMode == "rbCaller") srtMode = "caller";
        //    if (myConfig.strSRTMode == "rbListener") srtMode = "listener";
        //    if (myConfig.strSRTMode == "rbRendezvous") srtMode = "rendezvous";

        //    Process buildSRT = new Process();


        //    if (myConfig.strSRTMode == "inputUDP")
        //    {
        //        buildSRT.StartInfo.Arguments = "udp://" + myConfig.strUDPIP + ":" + myConfig.strUDPPort + " srt://" + myConfig.strSRTIP + ":" + myConfig.strSRTPort + "?mode=" + srtMode + "&latency=" + myConfig.strSRTLatency;
        //        Console.WriteLine(buildSRT.StartInfo.Arguments);
        //    }
        //    else
        //    {
        //        buildSRT.StartInfo.Arguments = "udp://:5555 srt://" + myConfig.strSRTIP + ":" + myConfig.strSRTPort + "?mode=" + srtMode + "&latency=" + myConfig.strSRTLatency;
        //        Console.WriteLine(buildSRT.StartInfo.Arguments);
        //    }

        //    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        //    {
        //        buildSRT.StartInfo.FileName = strSRTExe + ".exe";
        //    }
        //    else
        //    {
        //        buildSRT.StartInfo.FileName = strSRTExe;
        //    }

        //    //txtSRTCommand.Text = buildSRT.StartInfo.FileName + " " + buildSRT.StartInfo.Arguments.ToString();

        //    buildSRT.StartInfo.UseShellExecute = false;
        //    buildSRT.StartInfo.RedirectStandardOutput = true;
        //    buildSRT.StartInfo.RedirectStandardError = true;
        //    buildSRT.StartInfo.CreateNoWindow = true;
        //    //buildSRT.ErrorDataReceived += buildSRT_ErrorDataReceived;
        //    //buildSRT.OutputDataReceived += buildSRT_OutputDataReceived;
        //    buildSRT.EnableRaisingEvents = true;
        //    buildSRT.Start();
        //    //processID = build.Id;
        //    buildSRT.BeginOutputReadLine();
        //    buildSRT.BeginErrorReadLine();
        //}

        //private void buildSRT_OutputDataReceived(object sender, DataReceivedEventArgs e)
        //{
        //    if (e.Data != null)
        //    {
        //        //SendTextAsync("logSRTMsg|" + e.Data.ToString());
        //        ((WsServer)Server).MulticastText("logSRTMsg|" + e.Data.ToString());
        //    }


        //}

        //private void buildSRT_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        //{
        //    if (e.Data != null)
        //    {
        //        //SendTextAsync("logSRTMsg|" + e.Data.ToString());
        //        ((WsServer)Server).MulticastText("logSRTMsg|" + e.Data.ToString());
        //    }
        //}


        private void StartFFMPEG_Input(string udpIp, string udpPort, string strVideoCodec, string strVideoBitrate, string strAudioCodec, string strAudioBitrate)
        {
            string strFrameRate = "25";

            Process buildFFMPEG = new Process();

            //if (myConfig.strSelectedCard == "Decklink Not Found")
            //{
            //    return;
            //}
            //else if (myConfig.strInputType == "inputUDP")
            //{
                buildFFMPEG.StartInfo.Arguments = "-re -threads 8 -i \"" + "udp://@" + udpIp + ":" + udpPort + "\" -c:v  " + strVideoCodec + " -c:a " + strAudioCodec + " -r " + strFrameRate + " -b:a " + strAudioBitrate + "k -b:v " + strVideoBitrate + "M -preset ultrafast -intra -crf 20 -async 2 -maxrate " + strVideoBitrate + "M -bufsize " + strVideoBitrate + "M -pix_fmt yuv420p -x264opts nal-hrd=cbr:keyint=1 -tune zerolatency -aspect 16:9 -f mpegts \"udp://127.0.0.1:5555?pkt_size=1316\"";

                Console.WriteLine(buildFFMPEG.StartInfo.Arguments);
            //}
            //else if (myConfig.strInputType == "inputTest")
            //{
            //    //buildFFMPEG.StartInfo.Arguments = "-re -f lavfi -i smptebars=size=1920x1080:rate=25 -vf drawtext=fontfile=arial.ttf:text=\"" + System.Environment.MachineName + "\r\n\r\n" + @"srt'\://'" + myConfig.strSRTIP + @"'\:'" + myConfig.strSRTPort + "\":fontcolor=black:fontsize=100:x=(w-text_w)/2:y=(h-text_h)/2:borderw=5:bordercolor=white -c:v  " + strVCodec + " -c:a " + strACodec + " -r " + strFrameRate + " -b:a " + strABitrate + "k -b:v " + strVBitrate + "M -preset ultrafast -intra -crf 23 -x264opts nal-hrd=cbr:keyint=1 -tune zerolatency -r 25 -aspect 16:9 -f mpegts \"udp://127.0.0.1:5555?pkt_size=1316\"";
            //    buildFFMPEG.StartInfo.Arguments = "-re -f lavfi -i testsrc=1920x1080:rate=25  -c:v  " + strVCodec + " -c:a " + strACodec + " -r " + strFrameRate + " -b:a " + strABitrate + "k -b:v " + strVBitrate + "M -maxrate " + strVBitrate + "M -bufsize " + strVBitrate + "M -preset ultrafast -intra -crf 23 -x264opts nal-hrd=cbr:keyint=1 -tune zerolatency -aspect 16:9 -f mpegts \"udp://127.0.0.1:5555?pkt_size=1316\"";

            //    Console.WriteLine(buildFFMPEG.StartInfo.Arguments);
            //}
            //else
            //{
            //    buildFFMPEG.StartInfo.Arguments = "-re -threads 8 -format_code Hi50 -f decklink -i \"" + myConfig.strSelectedCard + "\" -c:v  " + strVCodec + " -c:a " + strACodec + " -r " + strFrameRate + " -b:a " + strABitrate + "k -b:v " + strVBitrate + "M -preset ultrafast -intra -crf 20 -async 2 -maxrate " + strVBitrate + "M -bufsize " + strVBitrate + "M -pix_fmt yuv420p -x264opts nal-hrd=cbr:keyint=1 -tune zerolatency -aspect 16:9 -f mpegts \"udp://127.0.0.1:5555?pkt_size=1316\"";
            //    Console.WriteLine(buildFFMPEG.StartInfo.Arguments);
            //}

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
            //buildFFMPEG.ErrorDataReceived += buildFFMPEG_ErrorDataReceived;
            //buildFFMPEG.OutputDataReceived += buildFFMPEG_OutputDataReceived;
            buildFFMPEG.EnableRaisingEvents = true;
            buildFFMPEG.Start();
            //processID = build.Id;
            buildFFMPEG.BeginOutputReadLine();
            buildFFMPEG.BeginErrorReadLine();
        }


        //private void StartFFMPEG_Input()
        //{

        //    //string strFormat = "Hi50";
        //    string strVCodec = "h264_nvenc";
        //    string strFrameRate = "25";
        //    string strACodec = "aac";
        //    string strABitrate = "128";
        //    string strVBitrate = "10000";

        //    Process buildFFMPEG = new Process();

        //    if (myConfig.strVideoCodec == "vcodecH264") strVCodec = "libx264";
        //    if (myConfig.strVideoCodec == "vcodecH265") strVCodec = "libx265";
        //    if (myConfig.strVideoCodec == "vcodecMPEG2") strVCodec = "mpeg2video";

        //    if (myConfig.strAudioCodec == "acodecAAC") strACodec = "aac";
        //    if (myConfig.strAudioCodec == "acodecMP3") strACodec = "mp3";
        //    if (myConfig.strAudioCodec == "acodecMPEG2") strACodec = "mp2audio";

        //    strVBitrate = myConfig.strVideoBitrate;
        //    strABitrate = myConfig.strAudioBitrate;

        //    if (myConfig.strSelectedCard == "Decklink Not Found")
        //    {
        //        return;
        //    }
        //    else if (myConfig.strInputType == "inputUDP")
        //    {
        //        buildFFMPEG.StartInfo.Arguments = "-re -threads 8 -i \"" + myConfig.strUDPIP + ":" + myConfig.strUDPPort + "\" -c:v  " + strVCodec + " -c:a " + strACodec + " -r " + strFrameRate + " -b:a " + strABitrate + "k -b:v " + strVBitrate + "M -preset ultrafast -intra -crf 20 -async 2 -maxrate " + strVBitrate + "M -bufsize " + strVBitrate + "M -pix_fmt yuv420p -x264opts nal-hrd=cbr:keyint=1 -tune zerolatency -aspect 16:9 -f mpegts \"udp://127.0.0.1:5555?pkt_size=1316\"";

        //        Console.WriteLine(buildFFMPEG.StartInfo.Arguments);
        //    }
        //    else if (myConfig.strInputType == "inputTest")
        //    {
        //        //buildFFMPEG.StartInfo.Arguments = "-re -f lavfi -i smptebars=size=1920x1080:rate=25 -vf drawtext=fontfile=arial.ttf:text=\"" + System.Environment.MachineName + "\r\n\r\n" + @"srt'\://'" + myConfig.strSRTIP + @"'\:'" + myConfig.strSRTPort + "\":fontcolor=black:fontsize=100:x=(w-text_w)/2:y=(h-text_h)/2:borderw=5:bordercolor=white -c:v  " + strVCodec + " -c:a " + strACodec + " -r " + strFrameRate + " -b:a " + strABitrate + "k -b:v " + strVBitrate + "M -preset ultrafast -intra -crf 23 -x264opts nal-hrd=cbr:keyint=1 -tune zerolatency -r 25 -aspect 16:9 -f mpegts \"udp://127.0.0.1:5555?pkt_size=1316\"";
        //        buildFFMPEG.StartInfo.Arguments = "-re -f lavfi -i testsrc=1920x1080:rate=25  -c:v  " + strVCodec + " -c:a " + strACodec + " -r " + strFrameRate + " -b:a " + strABitrate + "k -b:v " + strVBitrate + "M -maxrate " + strVBitrate + "M -bufsize " + strVBitrate + "M -preset ultrafast -intra -crf 23 -x264opts nal-hrd=cbr:keyint=1 -tune zerolatency -aspect 16:9 -f mpegts \"udp://127.0.0.1:5555?pkt_size=1316\"";

        //        Console.WriteLine(buildFFMPEG.StartInfo.Arguments);
        //    }
        //    else
        //    {
        //        buildFFMPEG.StartInfo.Arguments = "-re -threads 8 -format_code Hi50 -f decklink -i \"" + myConfig.strSelectedCard + "\" -c:v  " + strVCodec + " -c:a " + strACodec + " -r " + strFrameRate + " -b:a " + strABitrate + "k -b:v " + strVBitrate + "M -preset ultrafast -intra -crf 20 -async 2 -maxrate " + strVBitrate + "M -bufsize " + strVBitrate + "M -pix_fmt yuv420p -x264opts nal-hrd=cbr:keyint=1 -tune zerolatency -aspect 16:9 -f mpegts \"udp://127.0.0.1:5555?pkt_size=1316\"";
        //        Console.WriteLine(buildFFMPEG.StartInfo.Arguments);
        //    }

        //    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        //    {
        //        buildFFMPEG.StartInfo.FileName = strFFMPEGExe + ".exe";
        //    }
        //    else
        //    {
        //        buildFFMPEG.StartInfo.FileName = strFFMPEGExe;
        //    }

        //    buildFFMPEG.StartInfo.UseShellExecute = false;
        //    buildFFMPEG.StartInfo.RedirectStandardOutput = true;
        //    buildFFMPEG.StartInfo.RedirectStandardError = true;
        //    buildFFMPEG.StartInfo.CreateNoWindow = true;
        //    //buildFFMPEG.ErrorDataReceived += buildFFMPEG_ErrorDataReceived;
        //    //buildFFMPEG.OutputDataReceived += buildFFMPEG_OutputDataReceived;
        //    buildFFMPEG.EnableRaisingEvents = true;
        //    buildFFMPEG.Start();
        //    //processID = build.Id;
        //    buildFFMPEG.BeginOutputReadLine();
        //    buildFFMPEG.BeginErrorReadLine();
        //}


        //private void buildFFMPEG_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        //{
        //    if (e.Data != null)
        //    {
        //        //SendTextAsync("logEncoderMsg|" + e.Data.ToString());
        //        ((WsServer)Server).MulticastText("logEncoderMsg|" + e.Data.ToString());
        //    }
        //}

        //private void buildFFMPEG_OutputDataReceived(object sender, DataReceivedEventArgs e)
        //{
        //    if (e.Data != null)
        //    {
        //        //SendTextAsync("logEncoderMsg|" + e.Data.ToString());
        //        ((WsServer)Server).MulticastText("logEncoderMsg|" + e.Data.ToString());
        //    }
        //}
    }
}
