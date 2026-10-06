"""Export current recipes without requiring the old inspection pack."""
from pathlib import Path
import csv, json
root = Path(__file__).resolve().parent
recipes = []
for row in csv.DictReader((root / 'EncounterRecipes.csv').open(encoding='utf-8-sig')):
    recipes.append(dict(id=row['id'], name=row['name'], members=row['members'].split(';'),
                        **{key: int(row[key]) for key in ('first', 'last', 'tide', 'weight')}))
(root / 'Selected/encounters.json').write_text(json.dumps({'recipes': recipes}, indent=2) + '\n')
print(f'{len(recipes)} recipes exported; rationale: Docs/DROWNED_COURT_ENCOUNTERS.md')
