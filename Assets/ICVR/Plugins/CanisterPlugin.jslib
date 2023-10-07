var WebGLFunctions = {    

    ICLogin: function(cbIndex) {
		try {
			dispatchReactUnityEvent("ICLogin", cbIndex);
		} catch (e) {
			console.error("Test version detected; React not present");
		}
    },
    
    GetCoin: function(cbIndex) {
		try {
			dispatchReactUnityEvent("GetCoin", cbIndex);
		} catch (e) {
			console.error("Test version detected; React not present");
		}
    }

};

mergeInto(LibraryManager.library, WebGLFunctions);