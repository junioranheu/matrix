using Matrix.Domain.Enums;

namespace Matrix.Domain.Seeds;

public static class HumanNamesSeed
{
    public sealed class HumanNamePool
    {
        public required string[] MaleNames { get; init; }
        public required string[] FemaleNames { get; init; }
        public required string[] LastNames { get; init; }
    }

    public static class HumanNames
    {
        #region seed
        public static readonly IReadOnlyDictionary<HumanNameNationality, HumanNamePool> Pools = new Dictionary<HumanNameNationality, HumanNamePool>
        {
            [HumanNameNationality.Brazilian] = new()
            {
                MaleNames =
                [
                    "João","Pedro","Lucas","Gabriel","Mateus","Matheus","Miguel","Arthur","Davi","Bernardo",
                    "Samuel","Enzo","Benjamin","Joaquim","Lorenzo","Caio","Murilo","Otávio","Bruno","Felipe",
                    "Rafael","Henrique","Daniel","Diego","Carlos","Eduardo","Leonardo","Fernando","Ricardo","Rodrigo",
                    "Marcelo","Leandro","Vinicius","Igor","Alexandre","Paulo","Roberto","Antônio","Márcio","Fábio",
                    "Jonathan","William","Cristiano","Anderson","Alan","Breno","Cauã","Douglas","Emanuel","Erick",
                    "Francisco","Hugo","Ivan","Jefferson","Jonas","José","Kauan","Kevin","Luan","Luiz",
                    "Marcos","Maurício","Nathan","Nicolas","Patrick","Ramon","Raul","Robson","Sandro","Saulo",
                    "Sérgio","Thiago","Victor","Wesley","Yuri","Adriano","Alberto","Álvaro","Augusto","Benedito",
                    "Celso","Cláudio","Denis","Edgar","Elias","Everton","Fabrício","Frederico","Gilberto","Gilmar"
                ],
                FemaleNames =
                [
                    "Maria","Ana","Mariana","Julia","Beatriz","Laura","Alice","Sophia","Helena","Valentina",
                    "Isabella","Camila","Larissa","Amanda","Fernanda","Livia","Clara","Manuela","Luiza","Yasmin",
                    "Bianca","Natalia","Gabriela","Rafaela","Vitoria","Leticia","Lorena","Melissa","Milena","Eduarda",
                    "Isadora","Cecilia","Heloisa","Maite","Esther","Nicole","Raissa","Taina","Talita","Brenda",
                    "Tamara","Monica","Elisa","Ariana","Suzana","Rosana","Adriana","Alessandra","Barbara","Denise",
                    "Erika","Fabiana","Geovana","Ingrid","Janaina","Karla","Leila","Marina","Nayara","Pamela"
                ],
                LastNames =
                [
                    "Silva","Santos","Oliveira","Souza","Costa","Ferreira","Rodrigues","Almeida","Pereira","Gomes",
                    "Martins","Ribeiro","Carvalho","Rocha","Barbosa","Araujo","Melo","Freitas","Cardoso","Correia",
                    "Teixeira","Moura","Monteiro","Mendes","Batista","Dias","Campos","Castro","Nunes","Vieira",
                    "Machado","Andrade","Barros","Coelho","Fonseca","Queiroz","Assis","Sales","Pacheco","Xavier"
                ]
            },

            [HumanNameNationality.Western] = new()
            {
                MaleNames =
                [
                    "John","James","Michael","Robert","David","William","Joseph","Thomas","Charles","Christopher",
                    "Daniel","Matthew","Anthony","Andrew","Joshua","Ryan","Jacob","Noah","Logan","Ethan",
                    "Mason","Aiden","Jack","Luke","Wyatt","Owen","Levi","Hunter","Austin","Carter",
                    "Cooper","Connor","Tyler","Brandon","Jason","Nathan","Justin","Adam","Sean","Zachary"
                ],
                FemaleNames =
                [
                    "Emma","Olivia","Sophia","Charlotte","Amelia","Mia","Ava","Emily","Grace","Chloe",
                    "Abigail","Scarlett","Ella","Lily","Victoria","Hannah","Madison","Zoey","Natalie","Samantha",
                    "Ashley","Sarah","Claire","Audrey","Lucy","Anna","Ruby","Eva","Leah","Bella"
                ],
                LastNames =
                [
                    "Smith","Johnson","Williams","Brown","Jones","Miller","Davis","Wilson","Moore","Taylor",
                    "Anderson","Thomas","Jackson","White","Harris","Martin","Thompson","Garcia","Martinez","Robinson"
                ]
            },

            [HumanNameNationality.European] = new()
            {
                MaleNames =
                [
                    "Alexander","Lukas","Erik","Viktor","Nikolai","Ivan","Mikhail","Andreas","Felix","Matteo",
                    "Lorenzo","Marco","Pierre","Louis","Antoine","Thomas","Henrik","Magnus","Bjorn","Sven",
                    "Jan","Marek","Tomasz","Stefan","Milan","Aleksandar","Dmitri","Sergei","Anton","Leon"
                ],
                FemaleNames =
                [
                    "Anna","Sofia","Emma","Olivia","Elena","Anastasia","Natalia","Katarina","Isabella","Maria",
                    "Clara","Eva","Amelie","Camille","Elise","Ingrid","Freya","Astrid","Helena","Mila",
                    "Zofia","Tatiana","Irina","Alina","Nina","Victoria","Valentina","Bianca","Julia","Elena"
                ],
                LastNames =
                [
                    "Andersen","Johansson","Hansen","Nielsen","Eriksson",
                    "Schmidt","Muller","Weber","Fischer","Wagner",
                    "Dubois","Moreau","Laurent","Lefevre","Rousseau",
                    "Rossi","Ferrari","Russo","Bianchi","Romano",
                    "Novak","Kowalski","Nowak","Petrov","Ivanov"
                ]
            },

            [HumanNameNationality.Spanish] = new()
            {
                MaleNames =
                [
                    "Alejandro","Daniel","Pablo","Javier","Alvaro","Sergio","Hugo","Marcos","Diego","Adrian",
                    "Mateo","Tomas","Juan","Luis","Miguel","Fernando","Francisco","Carlos","Jorge","Antonio",
                    "Raul","Manuel","Eduardo","Ricardo","Andres","Gabriel","Angel","Emilio","Enrique","Esteban",
                    "Ignacio","Joaquin","Ramon","Salvador","Vicente","Cristian","Sebastian","Nicolas","Martin","Agustin"
                ],
                FemaleNames =
                [
                    "Lucia","Marta","Carmen","Paula","Laura","Sofia","Alba","Claudia","Elena","Irene",
                    "Valeria","Daniela","Sara","Julia","Marina","Noelia","Natalia","Patricia","Cristina","Silvia",
                    "Andrea","Camila","Isabel","Pilar","Teresa","Ana","Beatriz","Carolina","Veronica","Alicia"
                ],
                LastNames =
                [
                    "Garcia","Fernandez","Gonzalez","Rodriguez","Lopez","Martinez","Sanchez","Perez","Gomez","Martin",
                    "Jimenez","Ruiz","Hernandez","Diaz","Moreno","Munoz","Alvarez","Romero","Alonso","Navarro"
                ]
            },

            [HumanNameNationality.Arab] = new()
            {
                MaleNames =
                [
                    "Mohammed","Muhammad","Ahmed","Omar","Ali","Hassan","Hussein","Yousef","Yusuf","Khalid",
                    "Kareem","Mustafa","Ibrahim","Hamza","Bilal","Amir","Tariq","Nasser","Zaid","Faisal",
                    "Mahmoud","Abdullah","Abdul","Rashid","Samir","Karim","Jamal","Farid","Walid","Sami"
                ],
                FemaleNames =
                [
                    "Fatima","Aisha","Amina","Layla","Leila","Noor","Noura","Mariam","Maryam","Zahra",
                    "Yasmin","Salma","Samira","Rania","Farah","Sara","Hana","Lina","Nadia","Aya"
                ],
                LastNames =
                [
                    "Al-Hassan","Al-Farsi","Al-Saud","Al-Maktoum","Al-Rashid","Al-Najjar","Al-Amiri","Al-Khalifa","Al-Mansoori","Al-Harbi"
                ]
            },

            [HumanNameNationality.Asian] = new()
            {
                MaleNames =
                [
                    "Haruto","Yuto","Sota","Yuki","Ren","Kaito","Takumi","Daiki","Hiroto","Souta",
                    "Jisoo","Hyunwoo","Jiho","Seojun","Donghyun","Taeyang","Joon",
                    "Wei","Jun","Liang","Jian","Tao","Ming","Chen","Bo","Kai","Xin"
                ],
                FemaleNames =
                [
                    "Yui","Aoi","Rio","Hana","Mei","Yuna","Sakura","Haruka","Riko","Nao",
                    "Jiyeon","Minji","Soojin","Eunji","Yejin","Seoyeon","Jiwon",
                    "Ling","Meilin","Xia","Ying","Lan","Jing","Lian","Hui"
                ],
                LastNames =
                [
                    "Sato","Suzuki","Takahashi","Tanaka","Watanabe",
                    "Kim","Lee","Park","Choi","Jung",
                    "Wang","Li","Zhang","Liu","Chen"
                ]
            },

            [HumanNameNationality.African] = new()
            {
                MaleNames =
                [
                    "Kwame","Kofi","Kojo","Yaw","Adewale","Oluwaseun","Chinedu","Emeka","Obinna","Tunde",
                    "Jelani","Malik","Jabari","Amari","Kato","Bakari","Amani","Sekou","Thabo","Sipho",
                    "Mandla","Tumelo","Nkosana","Kagiso","Musa","Issa","Cheikh","Mamadou","Souleymane","Abdoulaye"
                ],
                FemaleNames =
                [
                    "Ama","Akosua","Abena","Adwoa","Zuri","Amina","Imani","Nia","Ayana","Makena",
                    "Chiamaka","Ngozi","Adaeze","Ifeoma","Thandiwe","Nomsa","Lerato","Zanele","Aisha","Fatou",
                    "Mariama","Awa","Nafissatou","Khadija","Safiya","Jamila","Eshe","Kamaria","Malaika","Nyah"
                ],
                LastNames =
                [
                    "Mensah","Boateng","Okafor","Adeyemi","Balogun",
                    "Ndlovu","Mbeki","Dlamini","Moyo","Khumalo",
                    "Diop","Fall","Sow","Diallo","Traore"
                ]
            },

            [HumanNameNationality.Indian] = new()
            {
                MaleNames =
                [
                    "Arjun","Rahul","Vikram","Rohan","Aarav","Aditya","Karan","Raj","Sanjay","Vivek",
                    "Anil","Ravi","Amit","Deepak","Nikhil","Akash","Varun","Manish","Harsh","Krishna",
                    "Yash","Ishaan","Pranav","Aryan","Kabir","Dev","Rajat","Suraj","Ajay","Sameer"
                ],
                FemaleNames =
                [
                    "Priya","Ananya","Aisha","Kavya","Pooja","Neha","Divya","Meera","Lakshmi","Saanvi",
                    "Aditi","Isha","Riya","Nisha","Shreya","Anjali","Simran","Kiran","Diya","Parvati",
                    "Sneha","Tara","Avni","Khushi","Jyoti","Radha","Maya","Rani","Vidya","Deepika"
                ],
                LastNames =
                [
                    "Patel","Sharma","Singh","Gupta","Kumar",
                    "Verma","Joshi","Mehta","Reddy","Nair",
                    "Kapoor","Malhotra","Chopra","Agarwal","Bose"
                ]
            },
        };
        #endregion
    }
}