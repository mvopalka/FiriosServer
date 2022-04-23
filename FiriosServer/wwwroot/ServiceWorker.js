self.addEventListener('fetch', function (event) { });
self.addEventListener('push', function (e) {
    var data;
    if (e.data) {
        var actions;
        data = JSON.parse(e.data.text());
        if (Notification.maxActions < 2) {
            actions = undefined;
        } else {
            actions = [
                {
                    action: "yes", title: "Jedu",
                    icon: "images/checkmark.png"
                },
                {
                    action: "no", title: "Nejedu",
                    icon: "images/red_x.png"
                }
            ];
        }

        var options = {
            body: data["message"],
            icon: "/images/icon-512x512.png",
            vibrate: [100, 50, 100],
            data: {
                id: data["id"],
                dateOfArrival: (new Date).toLocaleString(),
                smsSentAt: data["smsSentAt"],
                serverReceiveDate: data["serverReceiveDate"],
                url: self.location.origin,
                session: data["session"]
            },
            actions: actions
        };
        console.log(options.data);
        e.waitUntil(
            self.registration.showNotification("Push Notification", options)
        );
    }
});

self.addEventListener('notificationclick', function (e) {
    var notification = e.notification;
    var action = e.action;
    var data = notification.data;
    var urlButton = data["url"] + "/api/IncidentEntities/registration";
    var url = data["url"] + "/Home/UserConfirmAction/" + data["id"];

    function sendState(state) {
        const requestData = {
            "incidentId": data["id"],
            "state": state,
            "session": data["session"]
        };

        fetch(urlButton, {
                method: 'POST', // or 'PUT'
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(requestData),
            })
            .then(response => response.json())
            .then(data => {})
            .catch((error) => {
                clients.openWindow(url);
                notification.close();
            });
    }

    if (action === 'no') {
        sendState("no");
        notification.close();
    } else if (action === 'yes') {
        sendState("yes");
        notification.close();
    } else {
        // Some actions
        clients.openWindow(url);
        notification.close();
    }
});
