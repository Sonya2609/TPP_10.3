#�������� �������
def create():
  print('������� ����� ��� �������:')
  file.write(input())
  print('������� ���������')
def delete():
  print('������� �������, ������� ���� �������:')
def search():
  print('������� ������� ��� ������:')
def close():
  print('�� ��������!')
  exit()
def show():
  pass
#���������
def interface():
  print('����������, ������������! ��� �������� ����� �������.')
  while True:
    print('''��� ������ ��������� ������: 
    1) ������� �������
    2) ������� �������
    3) ����� �������
    4) ������� �������
    5) �������� �������
    ������� ����� ��������� �������:''')
    answer = input()
    match answer:
      case '1':
        create()
      case '2':
        delete()
      case '3':
        search()
      case '4':
        close()
      case '5':
        show()
      case _:
        print('������� ������������ ������')
        continue

interface()
file = open('memory.txt','r+')