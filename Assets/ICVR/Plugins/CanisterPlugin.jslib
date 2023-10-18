var WebGLFunctions = {    

    ICLogin: function(cbIndex) {
		try {
			dispatchReactUnityEvent("ICLogin", cbIndex);
		} catch (e) {
			console.error("Test version detected; React not present");
		}
    },
	
	ICLogout: function(cbIndex) {
		try {
			dispatchReactUnityEvent("ICLogout", cbIndex);
		} catch (e) {
			console.error("Test version detected; React not present");
		}
    },
    
    GetToken: function(cbIndex) {
		try {
			dispatchReactUnityEvent("GetToken", cbIndex);
		} catch (e) {
			console.error("Test version detected; React not present");
		}
    }

};

mergeInto(LibraryManager.library, WebGLFunctions);