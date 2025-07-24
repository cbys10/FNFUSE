import json

chart_data = {
    #used to prepare charts from the key to chart tool for game use!
}


  





HOLD_THRESHOLD = 0.4

def clean_notes(notes):
    for note in notes:
        if 'hold' in note and note['hold'] < HOLD_THRESHOLD:
            del note['hold']
    return notes

chart_data["bfNotes"] = clean_notes(chart_data["bfNotes"])
chart_data["opNotes"] = clean_notes(chart_data["opNotes"])

print(chart_data);
