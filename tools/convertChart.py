# Define the raw notes here
null = 0
raw_notes=[
    # converts fnf charts into fnfuse charts
]

# Mapping
bf_dir_map = {
    0: "left",
    1: "down",
    2: "up",
    3: "right",
}

op_dir_map = {
    4: "left",
    5: "down",
    6: "up",
    7: "right",
}

bfNotes = []
opNotes = []

for note in raw_notes:
    if note["d"] in bf_dir_map:
        altAni = ""
        if 'k' in note:
            altAni = "1"
        else:
            altAni = "0"
        converted_note = {
            "note": bf_dir_map[note["d"]],
            "time": note["t"] / 1000,
            "isAlt": altAni
        }
        if "l" in note:
            converted_note["hold"] = note["l"] / 1000
        bfNotes.append(converted_note)

    elif note["d"] in op_dir_map:
        altAni = ""
        if "k" in note:
            altAni = "1"
        else:
            altAni = "0"
        converted_note = {
            "note": op_dir_map[note["d"]],
            "time": note["t"] / 1000,
            "isAlt": altAni
        }
        if "l" in note:
            converted_note["hold"] = note["l"] / 1000
        opNotes.append(converted_note)

# Print results nicely
import json
print("bfNotes:")
print(json.dumps(bfNotes))
print("\nopNotes:")
print(json.dumps(opNotes))
