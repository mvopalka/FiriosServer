// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function getCookie(cname) {
    let name = cname + "=";
    let decodedCookie = decodeURIComponent(document.cookie);
    let ca = decodedCookie.split(";");
    for (let i = 0; i < ca.length; i++) {
        let c = ca[i];
        while (c.charAt(0) == " ") {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}

function getSession() {
    return getCookie("Session");
}

// Service worker registration

function remove_btn_outline(element, addClass) {
    if (element == null) {
        return;
    }
    element.classList.remove("btn-outline-primary");
    element.classList.remove("btn-outline-success");
    element.classList.remove("btn-outline-danger");
    element.classList.remove("btn-outline-warning");
    notification_top_bar.classList.add(addClass);

}

const notification_top_bar = document.getElementById("notificationTopMenu");

function noSupportPWA() {
    remove_btn_outline(notification_top_bar, "btn-outline-danger");
}

function noPermissionSupportPWA() {
    remove_btn_outline(notification_top_bar, "btn-outline-warning");
}

function SupportPWA() {
    remove_btn_outline(notification_top_bar, "btn-outline-success");
}

function DefaultPWA() {
    remove_btn_outline(notification_top_bar, "btn-outline-primary");
}

if ("serviceWorker" in navigator) {
    window.addEventListener("load", () => {
        navigator.serviceWorker.register("/ServiceWorker.js")
            .then((reg) => {
                if (Notification.permission === "granted") {
                    DefaultPWA();

                    getSubscription(reg);
                } else if (Notification.permission === "blocked" || Notification.permission === "denied") {
                    noSupportPWA();
                } else {
                    noPermissionSupportPWA();

                    notification_top_bar.addEventListener("click", () => requestNotificationAccess(reg));
                }
            }).catch(()=>noSupportPWA());
    });
} else {
    noSupportPWA();
}

function requestNotificationAccess(reg) {
    Notification.requestPermission(function (status) {
        DefaultPWA();
        if (status == "granted") {
            DefaultPWA();
            getSubscription(reg);
        } else {
            noSupportPWA();
        }
    });
}

function getSubscription(reg) {
    reg.pushManager.getSubscription().then(function (sub) {
        if (sub === null) {
            reg.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: document.getElementById("PWA_ServerKey").innerHTML
        }).then(function (sub) {
                SendPushServiceFields(sub);
            }).catch(function (e) {
                console.error("Unable to subscribe to push", e);
            });
        } else {
            SendPushServiceFields(sub);
        }
    });
}

function SendPushServiceFields(sub) {
    user_push_data = {
        "Endpoint": sub.endpoint,
        "P256dh": arrayBufferToBase64(sub.getKey("p256dh")),
        "Auth": arrayBufferToBase64(sub.getKey("auth")),
        "Session": getSession()
    };
    fetch("/api/IncidentEntities/PushRegistration",
            {
                method: "POST", // or 'PUT'
                headers: {
                    'Content-Type': "application/json",
                },
                body: JSON.stringify(user_push_data),
            })
        .then(response => {
            if (response.status === 200) {
                SupportPWA();
            } else {
                noSupportPWA();
            }
        })
        .catch(error => noSupportPWA());
}


function arrayBufferToBase64(buffer) {
    var binary = "";
    var bytes = new Uint8Array(buffer);
    var len = bytes.byteLength;
    for (var i = 0; i < len; i++) {
        binary += String.fromCharCode(bytes[i]);
    }
    return window.btoa(binary);
}

const session_name = "Session";

if (getSession() === "") {
    session_local = localStorage.getItem(session_name);
    if (session_local) {
        document.cookie = `${session_name}=${session_local};path=/`;
        location.reload();
    } else {
        localStorage.setItem(session_name, getSession());
    }
} else {
    localStorage.setItem(session_name, getSession());
}

