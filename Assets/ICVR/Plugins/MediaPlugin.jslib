mergeInto(LibraryManager.library, {

	openWindow: function(link) {
	var url = Pointer_stringify(link);
	document.onmouseup = function()
		{
			window.open(url, "_self");
			document.onmouseup = null;
		}
	},
  
  SaveAudioInIndexedDB : function (playerObject, path, idAudio) {
    var jPath = Pointer_stringify(path);
    var jIdAudio = Pointer_stringify(idAudio);
    var jPlayObj = Pointer_stringify(playerObject);
    
    window.indexedDB = window.indexedDB || window.webkitIndexedDB || window.mozIndexedDB || window.OIndexedDB || window.msIndexedDB,
    IDBTransaction = window.IDBTransaction || window.webkitIDBTransaction || window.OIDBTransaction || window.msIDBTransaction,
      dbVersion = 1.0;
    if (!window.indexedDB)
      {
        alert("Your browser doesn't support IndexedDB");
                      return -1;
    }
    
    var request = indexedDB.open("AudioFiles", dbVersion),
    db,
    createObjectStore = function (dataBase)
    {
        dataBase.createObjectStore("Soundtrack");
    },
    getAudioFile = function ()
    {
       var xhr = new XMLHttpRequest(),
       blob;
       // Get the Audeo file from the server.
       xhr.open("GET", jPath, true);  
       xhr.responseType = "blob";
       xhr.addEventListener("load", function ()
       {
          if (xhr.status === 200)
          {
              blob = xhr.response;
              console.log("SUCCESS: Audio file downloaded " + jIdAudio);
              putAudioInDb(blob);
			  
			  console.log("Got audio, sending to " + jPlayObj);
			  SendMessage(jPlayObj, "LoadAudioTrack", jIdAudio);
          }
          else
          {
              console.log("ERROR: Unable to download video.")
          }
        }, false);
        xhr.send();
	},
	
	putAudioInDb = function (blob) {
		var transaction = db.transaction(["Soundtrack"], "readwrite");
		var put = transaction.objectStore("Soundtrack").put(blob, jIdAudio);
    };
    
    request.onerror = function (event) {
		console.log("IndexedDB error: " + event.target.errorCode);
    };
  
    request.onsuccess = function (event) {
    console.log("Success creating/accessing IndexedDB database");
    db = request.result;
        db.onerror = function (event) {
            console.log("Error creating/accessing IndexedDB database");
        };      
        window.onload = getAudioFile();
    }
    
    request.onupgradeneeded = function (event) {
		createObjectStore(event.target.result);
    };
  },


  GetAudioUrlFromIndexedDB: function (goName, str) {
    console.log("attempting indexedDB read");
    var jGoName = Pointer_stringify(goName);
    var jStr = Pointer_stringify(str);
    console.log("Id : " + jStr);
    
    window.indexedDB = window.indexedDB || window.webkitIndexedDB || window.mozIndexedDB || window.OIndexedDB || window.msIndexedDB,
      IDBTransaction = window.IDBTransaction || window.webkitIDBTransaction || window.OIDBTransaction || window.msIDBTransaction,
      dbVersion = 1.0;
      
    if (!window.indexedDB)
    {
      alert("Your browser doesn't support IndexedDB");
    }

    var indexedDB = window.indexedDB;
    var request = indexedDB.open("AudioFiles", dbVersion);
   
    request.onerror = function (event) {
          console.log("IndexedDB error: " + event.target.errorCode);
    };
 
    request.onsuccess = function (event) {
      db = request.result;      
      // Open a transaction to the database
      var transaction = db.transaction(["Soundtrack"], "readwrite");
  
      // Retrieve the audio file
      transaction.objectStore("Soundtrack").get(jStr).onsuccess =
      function (event) {              
        var audioFile = event.target.result;
        var URL = window.URL || window.webkitURL;
        var audioURL = URL.createObjectURL(audioFile);
        console.log('audioURL created:', audioURL);
        SendMessage(jGoName, 'GetUrlFromWebGL', audioURL);	
      };
	  };
  }
  
    
});