# Stay! (Animals Wait for Handlers)

Tamed animals stop and wait when a handler is actively coming to train them, instead of wandering off and turning the training job into a chase.

## How it works

Checks for handlers doing a train/tame job and makes the target animal wait if it's walking off somewhere. The wait job expires on its own so nothing gets stuck.
Only affects animals you already own, and the handler has to be fairly close with line of sight.
No Harmony patches. It's just a MapComponent, so it should be compatible with basically everything and safe to add or remove mid-save.


