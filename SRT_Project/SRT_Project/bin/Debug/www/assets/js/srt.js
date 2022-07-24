  function init()
  {
    //document.myform.url.value = "ws://localhost:8080/"
    //document.myform.inputtext.value = "Hello World!"
    //document.myform.disconnectButton.disabled = true
  }

  function doConnect()
  {
    //websocket = new WebSocket(document.myform.url.value)
	websocket = new WebSocket("ws://127.0.0.1:8084/");
    websocket.onopen = function(evt) { onOpen(evt) };
    websocket.onclose = function(evt) { onClose(evt) };
    websocket.onmessage = function(evt) { onMessage(evt) };
    websocket.onerror = function(evt) { onError(evt) };
  }

  function onOpen(evt)
  {
    $("#statusBox").attr('class', 'alert alert-success');
    $("#statusBox").text("Connected");
    writeToScreen("connected\n")
    //document.myform.connectButton.disabled = true
    //document.myform.disconnectButton.disabled = false
	reqInputList();
	reqConfig();
  }

  function onClose(evt)
  {
    $("#statusBox").attr('class', 'alert alert-danger');
      $("#statusBox").text("Disconnected");
    writeToScreen("disconnected\n")
    //document.myform.connectButton.disabled = false
    //document.myform.disconnectButton.disabled = true
      
    setTimeout(function() {
      doConnect();
    }, 1000);
  }
            


  function onMessage(evt)
  {
    writeToScreen("response: " + evt.data + '\n');

	strRes = evt.data.split("|");



	if (strRes[0]=="respInputList")
	{
		respInputList(strRes[1]);
	}else if (strRes[0]=="respSettings")
	{
		respSettings(strRes[1]);
	}else if (strRes[0]=="respStats")
	{
		respStats(strRes[1]);
	}else if (strRes[0]=="logSRTMsg")
	{
		AddToSRTLog(strRes[1]);
	}else if (strRes[0]=="logEncoderMsg")
	{
		AddToEncoderLog(strRes[1]);
	}

  }

function AddToSRTLog(msg)
{
	$('#txtSRTLog').val(msg);
}

function AddToEncoderLog(msg)
{
	$('#txtEncoderLog').val(msg);
}



  function onError(evt)
  {
    writeToScreen('error: ' + evt.data + '\n');

    websocket.close()

    //document.myform.connectButton.disabled = false
    //document.myform.disconnectButton.disabled = true
  }

  function doSend(message)
  {
    writeToScreen("sent: " + message + '\n');
    websocket.send(message);
  }

function reqInputList()
	{
		doSend("reqInputList");
  }
  
function reAttach()
{
	doSend("reAttach");
}


function reqConfig()
{
	doSend("reqConfig");
}

  function reqSaveSettings()
  {
    doSend("reqSaveSettings|" +  $("#lstInputList option:selected").html() + "é" + $("#txtSRTIP").val() + "é" + $("#txtSRTPort").val() + "é" + $("#txtSRTLatency").val() + "é" + $('input[name=rbSRTMode]:checked').val()+ "é" + $('input[name=rbVideoCodec]:checked').val()+ "é" + $("#cbVideoBitrate option:selected").html()+ "é" + $('input[name=rbAudioCodec]:checked').val()+ "é" + $("#cbAudioBitrate option:selected").html() + "é" + $('input[name=rbInputType]:checked').val() + "é" + $("#txtUDPIP").val() + "é" + $("#txtUDPPort").val() + "é" + $("#txtSRTPassword").val());
  }

	function respInputList(strRes)
	{
		var $dropdown = $("#lstInputList");
		
		
		if (strRes=="nop")
		{
			$dropdown.empty();
			$dropdown.append("<option value='nop'>Decklink Card Not Found</option>");
			$dropdown.addClass("bg-danger");
			$dropdown.addClass("text-white");
		} else 
		{
      $dropdown.empty();

			$dropdown.addClass("bg-white");
			$dropdown.addClass("text-black");      

      var strSplit = strRes.split("é");

      strSplit.forEach(function(item){
        $dropdown.append("<option value='nop'>"  + item + "</option>");
      });

		

		}
	}


	function respStats(strRes)
	{
		console.log("Stat received");
		var strSplit = strRes.split("é");

		$("#lblCPU").html("CPU: " + (strSplit[0]) + "%"); 
		$("#lblRAM").html("MEM: " + (strSplit[1]) +	" MB"); 
	}


	function respSettings(strRes)
		{
			var strSplit = strRes.split("é");

			//inputTestéDecklink Card Not Foundé0.0.0.0é8888é0.0.0.0é8888é120émodeReceiveréévcodecH264é3éacodecAACé96

			// inputType
			if (strSplit[0] == "inputTest")
			{
				$('#rbTest').prop("checked", true);
			} else if (strSplit[0] == "inputSDI")
			{
				$('#rbSDI').prop("checked", true);
			}else if (strSplit[0] == "inputUDP")
			{
				$('#rbUDP').prop("checked", true);
			}


			// SelectCard
			if (strSplit[10] == "1")
			{
				$('#v1').prop("selected", true);
			}
			else if (strSplit[10] == "2")
			{
				$('#v2').prop("selected", true);
			}


			// UDPIP
			$("#txtUDPIP").val(strSplit[2]);
			// UDPPort
			$("#txtUDPPort").val(strSplit[3]);
			// SRTIP
			$("#txtSRTIP").val(strSplit[4]);
			// SRTPort
			$("#txtSRTPort").val(strSplit[5]);
			// SRTLatency
			$("#txtSRTLatency").val(strSplit[6]);

			// SRT Mode
			if (strSplit[7] == "modeListener")
			{
				$('#rbReceiver').prop("checked", true);
			} else if (strSplit[7] == "modeCaller")
			{
				$('#rbCaller').prop("checked", true);
			}else if (strSplit[7] == "modeRendezvous")
			{
				$('#rbRendezvous').prop("checked", true);
			}

			//SRT Password
			$("#txtSRTPassword").val(strSplit[8]);

			// Video Codec
			if (strSplit[9] == "vcodecH264")
			{
				$('#rbH264').prop("checked", true);
			} 
			else if (strSplit[9] == "vcodecH265")
			{
				$('#rbH265').prop("checked", true);
			}
			else if (strSplit[9] == "vcodecMPEG2")
			{
				$('#rbMPEG2').prop("checked", true);
			}

			// Video Bitrate

			$("#cbVideoBitrate").find("option").attr("selected", false);

			if (strSplit[10] == "1")
			{
				$('#v1').prop("selected", true);
			}
			else if (strSplit[10] == "2")
			{
				$('#v2').prop("selected", true);
			}
			else if (strSplit[10] == "3")
			{
				$('#v3').prop("selected", true);
			}
			else if (strSplit[10] == "4")
			{
				$('#v4').prop("selected", true);
			}			
			else if (strSplit[10] == "5")
			{
				$('#v5').prop("selected", true);
			}			
			else if (strSplit[10] == "10")
			{
				$('#v10').prop("selected", true);
			}			
			else if (strSplit[10] == "20")
			{
				$('#v20').prop("selected", true);
			}

			// Audio Codec
			if (strSplit[11] == "acodecAAC")
			{
				$('#rbAAC').prop("checked", true);
			} else if (strSplit[11] == "acodecMP3")
			{
				$('#rbMP3').prop("checked", true);
			}else if (strSplit[11] == "acodecMPEG2")
			{
				$('#rbMPEG2Audio').prop("checked", true);
			}

			// Audio Bitrate
			$("#cbAudioBitrate").find("option").attr("selected", false);
			if (strSplit[12] == "96")
			{
				$('#a96').prop("selected", true);
			} 
			else if (strSplit[12] == "128")
			{
				$('#a128').prop("selected", true);
			}
			else if (strSplit[12] == "256")
			{
				$('#a256').prop("selected", true);
			}
			else if (strSplit[12] == "384")
			{
				$('#a384').prop("selected", true);
			}
		}
	  
  
  function writeToScreen(message)
  {
    //document.myform.outputtext.value += message
    //document.myform.outputtext.scrollTop = document.myform.outputtext.scrollHeight
    //alert(message);
    console.log(message);
  }

  window.addEventListener("load", init, false)

  function sendText()
  {
    doSend(document.myform.inputtext.value);
  }

  function clearText()
  {
    document.myform.outputtext.value = "";
  }

  function doDisconnect()
  {
    websocket.close();
  }

doConnect();
