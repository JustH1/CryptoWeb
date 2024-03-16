//шифруем или дешифруем (true - encr, false-decr)
var encryptOrDecryp = true;
//форма
let form = document.querySelector("form");
//input выбора файла(ов)
let inputField = form.querySelector("#input-field");
//ul лист с файлами
let fileList = document.querySelector("#file-list");
//текстовое поле для вывода информации о количестве загруженных файлов
let countFiles = document.querySelector("#count-files");
//header Encryption
let encryptionLink = document.querySelector("#EncryptionLink");
//header Decryption
let decryptionLink = document.querySelector("#DecryptionLink");
//отправка файлов для работы на сервере
let sendFiles = document.querySelector("#send");
//Input for entering the password
let inputPasswd = document.querySelector("#input-passwd");

let fileArray = [];

//обработка события нажатия на кнопку отправки файлов
sendFiles.addEventListener("click", async () => {
    let formData = new FormData();

    var fileTypesEncryption = ["txt", "docx"];
    var fileTypesDecryption = ["bin"];

    if (encryptOrDecryp === true) {
        fileArray.forEach((element) => {
            if ((fileTypesEncryption.indexOf(element.name.split('.')[1]) > -1) === true) {
                formData.append(element.name, element);
            }
            else {
                alert("Unsupported file format.");
                return;
            }
        });
    }
    else {
        fileArray.forEach((element) => {
            let FileName = element.name.split('.')[1];
            if ((fileTypesDecryption.indexOf(FileName) > -1) == true) {
                formData.append(element.name, element);
            }
            else {
                alert("Unsupported file format.");
                return;
            }
        });
    }

    let Passwd;

    if (checkEmptyInputPass() === true) {
        Passwd = inputPasswd.value.trim();

        var response;

        if (encryptOrDecryp === true) {
            response = await fetch(`/Encrypt?pass=${Passwd}`, {
                method: "POST",
                body: formData,
            });
        }
        else {
            response = await fetch(`/Decrypt?pass=${Passwd}`, {
                method: "POST",
                body: formData,
            });
        }

        if (response.status = 200) {
            var FileName = (await response.text()).toString();
            var DownloadLink = document.createElement('a');
            console.log(FileName);
            DownloadLink.href = `/Download?type=${encryptOrDecryp}&name=${FileName}`;
            DownloadLink.download = FileName;
            DownloadLink.click();
            DownloadLink.remove();
        }
        else {
            alert("Status: " + response.status + ". Error: " + response.text());
        }
    }
    else {
        alert("Password is not to be empty.");
    }

});
//Проверка на пустой пароль
function checkEmptyInputPass() {
    if (inputPasswd.value.trim() === "") {
        return false;
    }
    return true;
}
//хотим шифровать
document.querySelector("#EncryptionLink").addEventListener("click", () => {
    if (!encryptionLink.classList.contains("done")) {
        encryptOrDecryp = true;
        encryptionLink.classList.toggle("done");
        decryptionLink.classList.remove("done");
    }
});
//хотим дешифровать
document.querySelector("#DecryptionLink").addEventListener("click", () => {
    if (!decryptionLink.classList.contains("done")) {
        encryptOrDecryp = false;
        encryptionLink.classList.remove("done");
        decryptionLink.classList.toggle("done");
    }
});

//вывод информации о ключе
document.querySelector("#passwd-info").addEventListener("click", () => {
    alert(
        "Ключ(пароль) является одним из наиболее важных элементов." +
        "Ключ key представляет собой секретное значение, которое используется для зашифрования и расшифрования данных" +
        "Ключ должен быть использован только для целей шифрования и расшифрования данных, для которых он был предназначен. ПОТЕРЯ КЛЮЧА ПРИВЕДЁТ К НЕВОЗМОЖНОСТИ ДЕШИФРОВКИ ФАЙЛА."
    );
});

form.addEventListener("click", () => {
    inputField.click();
});
//событие выбора файла в форме, добавляет файл в список
inputField.onchange = ({ target }) => {
    let files = target.files;
    for (let index = 0; index < files.length; index++) {
        if (files[index]) {
            addLi(files[index].name);
            fileArray.push(files[index]);
        }
    }
};

//функция добавления элемента списка li с именем файла
function addLi(fileName) {
    let LiElement = document.createElement("li");

    LiElement.innerHTML = `<i class="fa-solid fa-file"></i>
    <p>${fileName}</p><button><i class="fa-solid fa-xmark"></i></button>`;

    LiElement.querySelector("button").addEventListener("click", () => {
        if (Array.from(fileList.children).length == 1) {
            addEmpty();
        }
        let index = Array.from(fileList.children).indexOf(LiElement);
        fileArray.splice(index, 1);
        LiElement.remove();
    });

    countFiles.textContent = "Added files";
    fileList.appendChild(LiElement);
}

//функция изменения текста в countFile если список пуст
function addEmpty() {
    countFiles.textContent = "You haven't added any files yet.";
}
