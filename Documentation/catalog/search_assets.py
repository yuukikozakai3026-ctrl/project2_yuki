import argparse
import json
import re
import sys
import unicodedata
from pathlib import Path


def normalize(value):
    return unicodedata.normalize("NFKC", value).casefold().strip()


def matches(term, query):
    term, query = normalize(term), normalize(query)
    if not term:
        return False
    # English words must not match inside unrelated words (e.g. ui in building).
    if re.fullmatch(r"[a-z0-9 _\-/]+", term):
        return re.search(r"(?<![a-z0-9])" + re.escape(term)
                         + r"(?![a-z0-9])", query) is not None
    return term in query


def search(data, query, kind=None, top=3):
    scores, reasons, guidance = {}, {}, {}
    def score(pack_id, value, reason, note=None):
        scores[pack_id] = max(scores.get(pack_id, 0), value)
        reasons.setdefault(pack_id, set()).add(reason)
        if note:
            guidance.setdefault(pack_id, set()).add(note)

    for hint in data["lookup_hints"]:
        hits = [a for a in hint["aliases"] if matches(a, query)]
        if not hits:
            continue
        length = max(len(normalize(a)) for a in hits)
        exact = any(normalize(a) == normalize(query) for a in hits)
        base = (200 if exact else 120) + min(length, 30)
        for rank, key in enumerate(("primary_pack_ids", "secondary_pack_ids")):
            for pack_id in hint[key]:
                score(pack_id, base - 50 * rank, "/".join(hits), hint["note_ja"])

    for asset in data["assets"]:
        terms = [asset["name"], asset["id"], *asset["keywords_ja"],
                 *asset["keywords_en"], *asset["official_metadata"]["tags"]]
        for term in terms:
            if matches(term, query):
                exact = normalize(term) == normalize(query)
                score(asset["id"], (80 if exact else 40) + min(len(term), 20), term)

    found = [a for a in data["assets"] if scores.get(a["id"], 0) > 0
             and (kind is None or a["kind"] == kind)]
    found.sort(key=lambda a: (-scores[a["id"]], a["recommendation"]["priority"], a["id"]))
    return [{
        "id": a["id"], "name": a["name"], "kind": a["kind"],
        "url": a["source_url"],
        "matched": sorted(reasons[a["id"]]),
        "use": a["recommended_uses_ja"],
        "fit": a["recommendation"],
        "check": a["check_before_use_ja"],
        "query_note": sorted(guidance.get(a["id"], set())),
        "local": a["local"]
    } for a in found[:top]]


def main():
    parser = argparse.ArgumentParser(description="Kenney候補パックを短く検索")
    parser.add_argument("query")
    parser.add_argument("--catalog", default=str(Path(__file__).resolve().with_name("Kenney_Asset_Catalog.json")))
    parser.add_argument("--kind", choices=["model3d", "vfx2d", "ui2d", "audio", "texture"])
    parser.add_argument("--top", type=int, default=3)
    args = parser.parse_args()
    if not args.query.strip() or args.top < 1 or args.top > 10:
        parser.error("検索語は必須、--top は1〜10です")
    try:
        data = json.loads(Path(args.catalog).read_text(encoding="utf-8-sig"))
        results = search(data, args.query, args.kind, args.top)
    except (OSError, ValueError, KeyError, TypeError) as exc:
        parser.exit(2, "カタログ読込エラー: " + str(exc) + "\n")
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8")
    print(json.dumps({
        "query": args.query, "results": results,
        "note": "収録確定ではなく候補。0件でもKenney全体に存在しないとは言えない。"
    }, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
