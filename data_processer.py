import csv
import re

def fit_data(infile='./ETW-test.csv', outfile='./upd-ETW-test.csv', _range=(-1, 220)):
    upd_csv = []
    with open(infile, newline='') as csvfile:
        csvreader = csv.reader(csvfile, delimiter=';', skipinitialspace=True)
        c = 0
        for row in csvreader:
            if c > _range[0] and c < _range[1]:
                line = ['0' for i in range(20)]
                for i in range(20):
                    cell = re.sub(r'\s+', ' ', row[i]).strip()
                    if re.match(r'\d+,\d+E[+\-]\d*', str(cell)):
                        cell = get_float_exp(cell) 
                    if re.match(r'0x[0-7A-F]+', str(cell)):
                        cell = int(cell, 16)               
                    line[i] = cell if cell else '0'
                upd_csv.append(line)
            c+=1

    with open(outfile, 'w', newline='') as file:
        writer = csv.writer(file, delimiter=';', skipinitialspace=True)
        writer.writerows(upd_csv)

def get_float_exp(str_val):
    mantis, _range = str_val.replace(',', '.').split('E')
    _range = (-1) * float(_range[1::]) if _range[0] == '-' else float(_range[1::])
    val = float(mantis) * pow(10, _range)
    return val

    


if __name__ == '__main__':
    fit_data()