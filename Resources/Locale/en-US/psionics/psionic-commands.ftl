command-glimmershow-description = Show the current glimmer level.
command-glimmershow-help = No arguments.

command-glimmerset-description = Set glimmer to a number.
command-glimmerset-help = glimmerset (integer)

command-lspsionic-description = List psionics.
command-lspsionic-help = No arguments.

command-addpsionicpower-description = Initialize an entity as Psionic with a given PowerPrototype
command-addpsionicpower-help = Argument 1 must be an EntityUid, and argument 2 must be a string matching the PrototypeId of a PsionicPower.
addpsionicpower-args-one-error = Argument 1 must be an EntityUid
addpsionicpower-args-two-error = Argument 2 must match the PrototypeId of a PsionicPower

command-addrandompsionicpower-description = Initialize an entity as Psionic with a random PowerPrototype that is available for that entity to roll.
command-addrandompsionicpower-help = Argument 1 must be an EntityUid.
addrandompsionicpower-args-one-error = Argument 1 must be an EntityUid

command-removepsionicpower-description = Remove a Psionic power from an entity.
command-removepsionicpower-help = Argument 1 must be an EntityUid, and argument 2 must be a string matching the PrototypeId of a PsionicPower.
removepsionicpower-args-one-error = Argument 1 must be an EntityUid
removepsionicpower-args-two-error = Argument 2 must match the PrototypeId of a PsionicPower.
removepsionicpower-not-psionic-error = The target entity is not Psionic.
removepsionicpower-not-contains-error = The target entity does not have this PsionicPower.

command-removeallpsionicpowers-description = Remove all Psionic powers from an entity.
command-removeallpsionicpowers-help = Argument 1 must be an EntityUid.
removeallpsionicpowers-args-one-error = Argument 1 must be an EntityUid.
removeallpsionicpowers-not-psionic-error = The target entity is not Psionic.
