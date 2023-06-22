mergeInto(LibraryManager.library, {

	CreateNewConnection__deps: ['audioToAvatar'],
	CreateNewConnection: function (sender, socketURL, roomSize) {

		console.log("Starting WebRTC connection");
		this.unityNode = Pointer_stringify(sender);
		var jSocketURL = Pointer_stringify(socketURL);

		// ......................................................
		// ..................RTCMultiConnection Code.............
		// ......................................................

		var connection = new RTCMultiConnection();
		connection.socketURL = jSocketURL;
		connection.socketMessageEvent = 'RTCMultiConnection-Message';


		connection.session = {
			audio: false,
			video: false,
			data: true
		};

		connection.mediaConstraints = {
			audio: true,
			video: false
		};

		connection.sdpConstraints.mandatory = {
			OfferToReceiveAudio: true,
			OfferToReceiveVideo: false,
			VoiceActivityDetection: false,
			IceRestart: true
		};

		//connection.trickleIce = false;
		connection.autoCreateMediaElement = false;
		connection.autoCloseEntireSession = false;

		connection.maxParticipantsAllowed = roomSize;

		// https://www.rtcmulticonnection.org/docs/iceServers/
		// find non-google STUNs: https://raw.githubusercontent.com/pradt2/always-online-stun/master/valid_hosts.txt

		connection.candidates = {
			turn: true,
			stun: true
		};
		connection.iceTransportPolicy = 'all';

		connection.iceServers = [];

		connection.iceServers.push({
			urls: 'stun:stun.nexphone.ch:3478'
		});
		
		connection.iceServers.push({
			urls: 'stun:stun.threema.ch:3478'
		});

		connection.iceServers.push({
			urls: "turn:a.relay.metered.ca:80",
			username: "6b54310d192366596d5df7bc",
			credential: "gNhASBfoEeUWokJm",
		});

		connection.iceServers.push({
			urls: "turn:a.relay.metered.ca:80?transport=tcp",
			username: "6b54310d192366596d5df7bc",
			credential: "gNhASBfoEeUWokJm",
		});
		
		connection.iceServers.push({
			urls: "turn:a.relay.metered.ca:443",
			username: "fa4cd9890d762dfa224c98d8",
			credential: "ooCaXZjzUE9RC8sz",
		});

		connection.iceServers.push({
			urls: "turn:relay.metered.ca:443?transport=tcp",
			username: "fa4cd9890d762dfa224c98d8",
			credential: "ooCaXZjzUE9RC8sz",
		});



		// ......................................................
		// .......................Data Transfer..................
		// ......................................................

		connection.onopen = function (event) {
			// report the connection event to unity
			SendMessage(unityNode, "OnConnectedToNetwork", JSON.stringify(event));
		};

		connection.onclose = function (event) {

			// ensure the peer connection object is working
			if (typeof connection.peers[event.userid] === "undefined") {
				console.log("Disconnection flag:\n" + JSON.stringify(event));
				return;
			}

			// check for ICE 
			var pc = connection.peers[event.userid].peer;

			if (pc._lastConnectionState === "connected") {
				console.log("Connection dropped, restarting ice")
				pc.restartIce();
			}
			else {
				console.log("Disconnection event:\n" + JSON.stringify(pc));
				connection.disconnectWith(event.userid);
				connection.deletePeer(event.userid);

				// send event to unity
				SendMessage(unityNode, "OnDisconnectedFromNetwork", JSON.stringify(event));
			}
		};

		connection.onleave = function (event) {
			console.log("OnLeave flag:\n" + JSON.stringify(event));
			Object.keys(connection.streamEvents).forEach(function (streamid) {
				var streamEvent = connection.streamEvents[streamid];
				if (streamEvent.userid === event.userid) {
					streamEvent.stream.getAudioTracks().forEach(function (track) {
						console.log("Stopping audio track: " + JSON.stringify(track));
						track.enabled = false;
					});
					connection.onstreamended(streamEvent);
				}
			});
		};

		connection.onRoomFull = function (roomid) {
			console.log('Room is full, incrementing...');
			SendMessage(unityNode, "RoomIsFull", roomid);
		};

		connection.onUserStatusChanged = function (event) {
			var targetUserId = event.userid;

			if (event.status == 'offline') {
				console.log(targetUserId + " is now offline");
				SendMessage(unityNode, "RemoveAvatar", targetUserId);
			}
			else if (event.status == 'online') {
				console.log(targetUserId + " is now online");
				SendMessage(unityNode, "OnUserOnline", targetUserId);
			}

			connection.numberOfConnectedUsers = connection.getAllParticipants().length;
			console.log("numberOfConnectedUsers=" + connection.numberOfConnectedUsers);
		};

		connection.onstreamended = function (event) {
			console.log("Stream ended:" + JSON.stringify(event));
			/*
			connection.streamEvents.selectAll({
				userid: event.userid,
				remote: true,
				isAudio: true
			}).forEach(function(streamEvent) {
				streamEvent.stream.stop();
			});
			*/

			connection.numberOfConnectedUsers = connection.getAllParticipants().length;
		};

		connection.onmessage = function (event) {
			SendMessage(unityNode, "ReceivePoseData", JSON.stringify(event));
		};

		connection.onstream = function (event) {
			if (event.type === 'remote') {
				try {

					var audioEvent = event;
					var audioCtx = new AudioContext();
					var audioMimeType = 'audio/webm';

					// identify mediastream object
					var mediaStream = connection.streamEvents[event.streamid].stream;

					console.log("OnStream::" + event.streamid + ",\n" + JSON.stringify(event.stream));

					// create media recorder 
					var options = {
						mimeType: audioMimeType,
						audioBitsPerSecond: audioCtx.sampleRate
					}
					var mediaRecorder = new MediaRecorder(mediaStream, options);
					var audioBlob = null;
					var noChunks = 0;
					var header = null;

					// configure data callback
					mediaRecorder.ondataavailable = function (event) {
						noChunks++;
						if (noChunks == 1) {
							audioBlob = new Blob([event.data], { 'type': audioMimeType });
							header = audioBlob.slice(0, 264);
						}
						else {
							audioBlob = new Blob([header, event.data.slice(4, -1)], { 'type': audioMimeType });
						}

						var audioURL = URL.createObjectURL(audioBlob);
						audioEvent.mediaElement.URL = audioURL;

						_audioToAvatar(audioEvent);
					}

					mediaStream.onremovetrack = function (event) {
						console.log("Removed track: " + event.track.kind + ":" + event.track.label);
						mediaRecorder.stop();
						noChunks = 0;
					};

					// collect the stream
					mediaRecorder.start(1000);

				}
				catch (e) {
					console.log("Broken audio stream \n" + e);
				}
			}
		};

		this.connection = connection;

		SendMessage(unityNode, 'OnFinishedLoadingRTC', JSON.stringify(connection));
	},

	audioToAvatar: function (event) {

		if (connection.numberOfConnectedUsers >= 1) {
			SendMessage(event.userid, "AddAudioStream", JSON.stringify(event));
		}
	},

	function() {
		var params = {},
			r = /([^&=]+)=?([^&]*)/g;

		function d(s) {
			return decodeURIComponent(s.replace(/\+/g, ' '));
		}
		var match, search = window.location.search;
		while (match = r.exec(search.substring(1)))
			params[d(match[1])] = d(match[2]);
		window.params = params;
	},


	// Interface
	StartConnection: function (roomId) {
		try {
			var jRoomId = Pointer_stringify(roomId);
			connection.openOrJoin(jRoomId, function (IsRoomJoined, jRoomId, error) {
				if (error == 'Room full') {
					SendMessage(unityNode, "RoomIsFull", jRoomId);
					return;
				}
			});

			SendMessage(unityNode, "OnConnectionStarted", jRoomId);
		}
		catch (e) {
			console.log("Error starting connection:\n" + e);
		}

	},

	CeaseConnection: function () {
		// disconnect with all users
		connection.getAllParticipants().forEach(function (pid) {
			connection.disconnectWith(pid);
		});

		// stop all local streams
		connection.attachStreams.forEach(function (localStream) {
			localStream.stop();
		});

		// close socket.io connection
		connection.closeSocket();
	},

	SendData: function (params) {
		try {
			var jMsg = Pointer_stringify(params);
			connection.send(jMsg);
		}
		catch (e) {
			console.log("Error sending message\n params:" + stringify(params) + '\n' + e);
		}
	},

	/*
	createOffer: function(userId) {
		connection.createOffer().then(function(offer) {
			return connection.setLocalDescription(offer);
			})
			.then(function() {
				SendData({
					target: userId,
					type: "audio-offer",
					sdp: connection.localDescription
				});
			})
			.catch(function(reason) {
				console.log("offer creation failed:" + reason);
		});
	},
	*/

	StartAudioStream: function (userId) {

		var jUserId = Pointer_stringify(userId);
		var isStreaming = false;


		try {

			Object.keys(connection.streamEvents).forEach(function (streamid) {

				var event = connection.streamEvents[streamid];

				if (event.userid === jUserId && event.stream.isAudio) {

					//console.log("unmuting " + jUserId);
					//event.stream.unmute('audio');


					var pc = connection.peers[jUserId].peer;
					console.log("resending audio to " + jUserId);



					var micOptions = {
						audio: true,
						video: false
					};

					connection.captureUserMedia(function (microphone) {
						var streamEvent = {
							type: 'local',
							stream: microphone,
							streamid: microphone.id
						};

						// only use the first found streamevent
						if (!isStreaming) {
							connection.onstream(streamEvent);
							isStreaming = true;
						}

						connection.dontCaptureUserMedia = true;

						connection.session = {
							audio: true,
							video: false,
							data: true
						};

						connection.attachStreams.forEach(function (localStream) {
							//console.log("adding new stream:" + JSON.stringify(localStream));
							//pc.addStream(localStream);

							streamEvent.stream.getAudioTracks().forEach(function (track) {
								console.log("adding new track:" + JSON.stringify(track));
								pc.addTrack(track, localStream);
							});
						});

					}, micOptions);

					connection.sdpConstraints.mandatory = {
						OfferToReceiveAudio: true,
						OfferToReceiveVideo: false,
						VoiceActivityDetection: false,
						IceRestart: true
					};

					connection.mediaConstraints = {
						video: false,
						audio: true
					};

					connection.peers[jUserId].addStream({
						data: true,
						audio: true,
						oneway: true
					});


					connection.renegotiate(jUserId);
					console.log("renegotiated connection");


				}
			});

			if (!isStreaming) {

				console.log("sending fresh audio to " + jUserId);

				connection.session = {
					audio: true,
					video: false,
					data: true
				};

				connection.sdpConstraints.mandatory = {
					OfferToReceiveAudio: true,
					OfferToReceiveVideo: false,
					VoiceActivityDetection: false,
					IceRestart: true
				};

				connection.mediaConstraints = {
					video: false,
					audio: true
				};

				connection.peers[jUserId].addStream({
					data: true,
					audio: true,
					oneway: true
				});
			}

		}
		catch (e) {
			console.log("error adding audio stream: " + e);
		}


	},

	StopAudioStream: function (userId) {
		var jUserId = Pointer_stringify(userId);

		connection.streamEvents.selectAll({
			userid: jUserId,
			remote: true,
			isAudio: true
		}).forEach(function (streamEvent) {

			console.log("muting " + jUserId);
			//streamEvent.stream.mute('audio');

			streamEvent.stream.getTracks().forEach(function (track) {
				track.stop();
			});

			streamEvent.stream.getTracks().forEach(function (track) {
				streamEvent.stream.removeTrack(track);
			});

		});

		//connection.renegotiate(jUserId);

	},

});