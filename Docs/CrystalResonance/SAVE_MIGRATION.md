# Resonance continuation contract

The outer combat snapshot retains its version 1 seconds / version 2 accepted-move
meaning. Nested `resonance.version=1` owns the new progression fields. Absent/zero
is legacy, never interpreted as a charged crystal. Future or inconsistent nested
versions preserve the save and stop restoration with the existing error UI.

Legacy active fights start with zero visit charge and no retroactive card queue;
the current fight may earn one victory award. An already-settled legacy wave is
marked banked. Existing visible card offers remain selectable once. Seen enemy
names migrate to mechanic/unique-leader IDs. Existing card stacks, score-independent
account rewards, HP, shield, energy, cooldowns and accepted-move clocks retain their
existing owners. No previous hidden score is invented.

An already-earned legacy stage-1 departure may finish (full/defeated migration
marker). A legacy stage-2 arrival begins at zero in its destination. New travel
requires settled 100 charge, the locked capstone's whole formation defeated and
all queued cards consumed. Destination RNG selection and save-before-smoke are
unchanged. Only the detached atomic destination checkpoint resets visit charge,
notches, encounter mastery, threshold and capstone. Cumulative score, receipt
cursor, leader/mechanic history and global wave are retained. Failed storage
keeps the visible source and durable checkpoint unchanged; a save gate prevents
next-wave leakage after banking/card completion.

Source cleanup still belongs to BoardController.PrepareZoneArrival: vine/root,
water, ice, mine-stone/drill and crumble state are reset by their established
fields; ordinary gem IDs and player specials survive. Zone holes use saved refill
randomness. No broad status purge or player heal was added.

Source implementation only. Save/reload, failed-write, card and transition
regressions are deferred to Stage 10; none have been executed at this checkpoint.
