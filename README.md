# Scene view notification
Notification log for Unity's scene view.

Instructions
------------

Call the static function `SceneViewNotification.Add(string text, NotificationType type)` from an editor script to display a message. `NotificationType` can be `Info, Warning, Error`.

![alt text](https://i.imgur.com/khUofTA.gif "")

Under `Edit->Preferences` you can configure the maximum lifetime of a message, and the time it takes to fade one out.

![alt text](https://i.imgur.com/0UGSaM3.png "")

License
-------

MIT License (see [LICENSE](LICENSE.md))