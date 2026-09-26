# Courier Crusaders

You run a fantasy courier company, hiring couriers and sending them down dangerous roads to make deliveries before the money runs out.

- Play: [itch.io](https://unitedfailures.itch.io/ludum-dare-53)
- Made: April to June 2023. It started as our Ludum Dare 53 entry.
- Team: [@mfclinton](https://github.com/mfclinton) (programming), [@DavidKoleczek](https://github.com/DavidKoleczek) (programming)
- Engine: Unity, C#

This is an export of a private repo with only the code we wrote. Art, audio, the Unity project files, and third party plugins aren't included. The history is squashed into one commit.

## What I built

- I wrote the math behind events on the road. Each kind of event, like a battle or a crime, can test a few different stats, and the party picks which one to use. Smarter parties are better at picking their best stat. The game works out the exact chance of success from all of that and shows it for each kind of event while you build your party.
- Parties travel the routes you plan on the map at a speed based on their dexterity and size. Events sit at spots along the roads and trigger when a party passes one.
- The courier who gets caught up in an event is picked by their stats, so your best fighter usually ends up in the battle.
