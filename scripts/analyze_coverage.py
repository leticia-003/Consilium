#!/usr/bin/env python3
import xml.etree.ElementTree as ET
import sys
from pathlib import Path

def analyze(covfile):
    tree = ET.parse(covfile)
    root = tree.getroot()
    classes = []
    for pkg in root.findall('./packages/package'):
        for cls in pkg.findall('./classes/class'):
            name = cls.get('name')
            filename = cls.get('filename')
            line_rate = float(cls.get('line-rate') or 0)
            # Sum lines valid for this class
            lines_valid = len(cls.findall('./lines/line'))
            classes.append((line_rate, lines_valid, filename, name))
    # Sort by line_rate ascending, then by lines_valid descending
    classes.sort(key=lambda x: (x[0], -x[1]))
    print("Top 30 lowest coverage classes (line-rate, lines-valid, filename):")
    for r, lv, f, n in classes[:30]:
        print(f"{r:.2f}  {lv:4d}  {f}")

if __name__ == '__main__':
    cov = sys.argv[1] if len(sys.argv) > 1 else 'src/TestResults/*/coverage.cobertura.xml'
    from glob import glob
    files = glob(cov)
    if not files:
        print('No coverage file found matching', cov)
        sys.exit(1)
    analyze(files[0])
