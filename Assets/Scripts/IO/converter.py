import argparse
import os
import sys

import twmap


def run():
    parser = argparse.ArgumentParser(description="tw map mapDir to .map converter")
    parser.add_argument("-i", type=str, help="path to mapDir map")
    parser.add_argument("-o", type=str, help="output path for .map file")
    args = vars(parser.parse_args())

    map_path = os.path.normpath(args['i'])
    map_name = os.path.basename(map_path)

    print(f'parsing {map_path}')
    m = twmap.Map(map_path)  # read map in MapDir format
    m.save(args['o'])  # export map as .map
    print(f"exported as {args['o']}")


if __name__ == '__main__':
    run()
