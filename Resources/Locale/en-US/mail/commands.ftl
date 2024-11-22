# Mailto
command-mailto-description = Queue a parcel to be delivered to an entity. Example usage: `mailto 1234 5678 false false`. The target container's contents will be transferred to an actual mail parcel.
command-mailto-help = Usage: {$command} <recipient entityUid> <container entityUid> [is-fragile: true or false] [is-priority: true or false] [is-large: true or false, optional]
command-mailto-no-mailreceiver = Target recipient entity does not have a {$requiredComponent}.
command-mailto-no-blankmail = The {$blankMail} prototype doesn't exist. Something is very wrong. Contact a programmer.
command-mailto-bogus-mail = {$blankMail} did not have {$requiredMailComponent}. Something is very wrong. Contact a programmer.
command-mailto-invalid-container = Target container entity does not have a {$requiredContainer} container.
command-mailto-unable-to-receive = Target recipient entity was unable to be setup for receiving mail. ID may be missing.
command-mailto-no-teleporter-found = Target recipient entity was unable to be matched to any station's mail teleporter. Recipient may be off-station.
command-mailto-success = Success! Mail parcel has been queued for next teleport in {$timeToTeleport} seconds.

# Mailnow
command-mailnow = Force all mail teleporters to deliver another round of mail as soon as possible. This will not bypass the undelivered mail limit.
command-mailnow-help = Usage: {$command}
command-mailnow-success = Success! All mail teleporters will be delivering another round of mail soon.

# Mailtestbulk
command-mailtestbulk = Sends one of each type of parcel to a given mail teleporter.  Implicitly calls mailnow.
command-mailtestbulk-help = Usage: {$command} <teleporter_id>
command-mailtestbulk-success = Success! All mail teleporters will be delivering another round of mail soon.
