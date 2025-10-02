#создание заметок
def create():
  print('Введите текст для заметки:')
  file.write(input())
  print('Заметка добавлена')
def delete():
  print('Введите заметку, которую надо удалить:')
def search():
  print('Введите заметку для поиска:')
def close():
  print('До свидания!')
  exit()
def show():
  pass
#интерфейс
def interface():
  print('Здравствуй, пользователь! Это менеджер твоих заметок.')
  while True:
    print('''Вот список доступных команд: 
    1) создать заметку
    2) удалить заметку
    3) поиск заметки
    4) закрыть заметку
    5) показать заметку
    Введите номер выбранной команды:''')
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
        print('Введены некорректные данные')
        continue

interface()
file = open('memory.txt','r+')