*****************
ReminderCommand
*****************

addlsnr
---------------
Adds a new listener to a bot reminder

.. code::

	s]addlsnr (bot) [user]

Examples:

- ``s]addlist MyBot``
  Assigns yourself to the MyBot reminder
- ``s]addlist MyBot Han``
  Assigns another user named Han to the MyBot reminder

....

addrmd
---------------
Adds a new bot reminder to the list

usage: `s]addrmd (bot) [duration]`

Examples:

- ``s]addrmd MyBot``
  Adds a bot to be tracked for it's offline status. Duration is 0 seconds by default, once the bot is offline, StatusBot immediately pings all the listener of the reminders
- ``s]addrmd MyBot 2s``
  Adds as well as setting the duration to 2 seconds

....

dellsnr
---------------
Removes a listener from a bot reminder

.. code::

	s]dellsnr (bot) [user]

Examples:

- ``s]dellist MyBot``
  Removes yourself from the MyBot reminder
- ``s]dellist MyBot Solo``
  Removes another user named Solo from the MyBot reminder

....

delrmd
---------------
Removes a bot reminder from the list.

.. code::

	s]delrmd (bot)

Example:

``s]delrmd MyBot`` MyBot's offline status will not be tracked by StatusBot

....

durationrmd
---------------
Modifies the duration of a bot reminder.

.. code::

	s]durationrmd (bot) [duration]

Examples:

- ``s]durationrmd MyBot``
  Sets the reminder delay duration to 0 seconds (default)
- ``s]durationrmd MyBot 12s``
  Sets the reminder delay duration to 12 seconds

....

rmd
---------------
Lists all the bot reminders in the current server, or lists all the listeners of a specified bot reminder

.. code::

	s]rmd [bot]

Examples:

- ``s]rmd``
  Lists all the bot reminders in the current server
- ``s]rmd okBot``
  Lists all the listeners of okBot

....

switchrmd
---------------
Activates or deactivates a bot reminder.

.. code::

	s]switchrmd (bot) < off | on >

Examples:

- ``s]switchrmd MyBot on``
  Activates the reminder for MyBot
- ``s]switchrmd MyBot off``
  Deactivates the reminder for MyBot

