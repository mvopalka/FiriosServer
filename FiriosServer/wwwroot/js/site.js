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
    fetch('/api/IncidentEntities/PushRegistration',
            {
                method: 'POST', // or 'PUT'
                headers: {
                    'Content-Type': 'application/json',
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
    var binary = '';
    var bytes = new Uint8Array(buffer);
    var len = bytes.byteLength;
    for (var i = 0; i < len; i++) {
        binary += String.fromCharCode(bytes[i]);
    }
    return window.btoa(binary);
}

renderCard = (data) => {
    const regexExp = /^[0-9a-fA-F]{8}\b-[0-9a-fA-F]{4}\b-[0-9a-fA-F]{4}\b-[0-9a-fA-F]{4}\b-[0-9a-fA-F]{12}$/gi;
    if (!regexExp.test(data.Id)) {
        return null;
    }
    try {
        document.getElementById(data.Id).remove();
    } catch (e) {

    }
    dt_name = document.createElement("dt");
    dt_name.innerText = "Jméno:";
    dd_name = document.createElement("dd");
    dd_name.innerText = data.Name;
    dt_surname = document.createElement("dt");
    dt_surname.innerText = "Příjmení:";
    dd_surname = document.createElement("dd");
    dd_surname.innerText = data.Surname;
    dt_position = document.createElement("dt");
    dt_position.innerText = "Pozice:";
    dd_position = document.createElement("dd");
    dd_position.innerText = data.Position;
    dl = document.createElement("dl");
    dl.appendChild(dt_name);
    dl.appendChild(dd_name);
    dl.appendChild(dt_surname);
    dl.appendChild(dd_surname);
    dl.appendChild(dt_position);
    dl.appendChild(dd_position);

    card = document.createElement("div");
    card.classList.add("card");
    card.id = data.Id;
    cardBody = document.createElement("div");
    cardBody.classList.add("card-body");

    cardBody.appendChild(dl);
    card.appendChild(cardBody);
    document.getElementById(data.Action).appendChild(card);
};
