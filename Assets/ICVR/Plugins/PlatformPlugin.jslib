mergeInto(LibraryManager.library, {
    DetectFormFactor: function(goName) {
        var formFactor = 'Unknown';
        var hasTouchScreen = false;
		var jGoName = Pointer_stringify(goName);

        // Check for touch capabilities
        if ("maxTouchPoints" in navigator) {
            hasTouchScreen = navigator.maxTouchPoints > 0;
        } else if ("msMaxTouchPoints" in navigator) {
            hasTouchScreen = navigator.msMaxTouchPoints > 0;
		} else if ('ontouchstart' in window) {
			hasTouchScreen = true;
        } else {
            var mQ = window.matchMedia && matchMedia("(pointer:coarse)");
            if (mQ && mQ.media === "(pointer:coarse)") {
                hasTouchScreen = !!mQ.matches;
            } else if ('orientation' in window) {
                hasTouchScreen = true; // deprecated, but good fallback
            } else {
                // Only as a last resort, fall back to user agent sniffing
                var UA = navigator.userAgent;
                hasTouchScreen = (
                    /\b(BlackBerry|webOS|iPhone|IEMobile)\b/i.test(UA) ||
                    /\b(Android|Windows Phone|iPad|iPod)\b/i.test(UA)
                );
            }
        }

        if (hasTouchScreen) {
			console.log("touchscreen detected");
            formFactor = 'Mobile';
        }
		
		// Check for headset
		navigator.mediaDevices.enumerateDevices()
			.then(function(devices) {
				devices.forEach(function(device) {
					if (device.kind === 'audioinput' && 
					device.label.toLowerCase().includes('headset')) {
						formFactor = 'HMD';
					}
				});
				// Assuming if not mobile and not headset, it's a PC
				if (formFactor === 'Unknown') {
					formFactor = 'PC';
				}
				// Send the result back to Unity
				SendMessage(jGoName, 'FormFactorResult', formFactor);
			})
			.catch(function(err) {
				console.error('Error enumerating devices:', err);
				// Assuming if there's an error, it's a PC
				formFactor = 'PC';
				SendMessage(jGoName, 'FormFactorResult', formFactor);
		});
    }
});
