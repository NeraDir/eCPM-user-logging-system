const admin = require("firebase-admin");

admin.initializeApp({
  credential: admin.credential.cert({
    "type": "service_account",
    "project_id": "examplecpm-41336",
    "private_key_id": "191c82dee88b01d13615a721e46f842d7d7c118f",
    "private_key": "-----BEGIN PRIVATE KEY-----\nMIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQDhW1DE/CEVOkRh\no7rDEM1uyN60hh3YaPuRtSflPmprb8fseL8p8+pcON+zjCmH3RIfVnuHvV9F67vW\nPYtEoXfDKcZPCXofkDdyiMbjlqJVyGi378f4EqBHBNaj+XkSNfHaKEZOSNI66aB8\nzvx1i+CMVRpqRhmx7EqSI+3Gc4PaZdo7KWUOtPBcorhP1+9l2dcgkmmVj5cnMS53\nwHuaMZemN2vA4xDUoWe4YBPCwZPy2AeD4LzP/Dmcd8LayK7iWCvdFrZMJlcqpFBE\nQ51bYw7WbOPmGfn5w+vY8nWjF/ce46Lyx90cNGDAIBY31iQ5Tdt3/ZVEi673MvoL\ncOH9V3ZvAgMBAAECggEAOcvZoyumsHMJJqF/PJgKl+ccduAOt3n1SyFS4hBn6rX4\naEVvgar06EIN3VaNMDMaer8LX3zJ+nzC5+dQ2IEw3fTQz2R21bF/BvjdSXFQoP+N\nG997QbxFLvGmutc4ndsH9BNwNJOol0NjzZ6oUN1W8abB54+i92bKmUBtsv/dSR1q\n51xiPPhOnTybY283bkOOsqizs7o8ZXny34S6JqSEClCBhMjxXii16qwuojPQ3JK9\nSe8+Yw8mawDnIzg3k0/BhAVaS5DygfAhvxq89AcRfDbwHI02aLiluExNW06YQrow\nik+GVjmiVvuZnHzUW9OoPoxMAmDCqxGkY0iV17eNwQKBgQD1ew8fvvW1B7PHMMYU\nKqF6/NIH6X3gPjzBdDkJY8vvPevn3a2GLh523uIPftksfUfnDvMglB/mcueyQ5lU\nUeIIQLi45n3Wo9mYyC36pjvrTwNoKYkhw7F2L3AxriMwcJdiZxtyrctUpWCqNrGy\nvguCIRWLxx6P0Ex9VIoWuFXx3QKBgQDrA36l6lTtQ/KWd32lqDo74J+CBBLuWsjr\n1XojsR34mbINQATUvzb33XsxvRunqqIhudMK9Rrue/EFrvR1j380Pwy+Y2pjugyU\nZ4vD3mXOTt5j7F7XoiofIvWRdRQogO79Vumx3iTtJWE5rjA/4T6hrs+Bxdo4Ma3W\nMmDeB4NSuwKBgHhK5F9nIVqjuppKsVYiid0N1RJhffiMJxOZk3WhfMbw7hBCVg0h\nvnX7xIVZYKzne3ujnMqDK5qvBhEaBSIHuh7XMG4TLOkog7HVropcOZKWtpGtgPCV\nx/mlR3Jg5gePtO9YnV+2AKSrIdJnWO7BKDyNX9SOL9DDfygPbFez3xHVAoGBAOZu\nR8jg6iK2Q+8a/HzoZSj+xweRfQpuw6DRcW/7o3p1IOrzAYmkb347kSak2K/elBhj\nfrW4vI5nwlyjPhji+YO96n3nNpLQArOkj5sevk26cT4irp/Z5lkfSw8zb++C9FI8\n3OlE+on348vNqouIDIQ1xkrlN2Fv/JYQUYCCd1YvAoGBAMLhpzsse+l4NZmFZz3d\nc2ro3Sbh/2by5fDklgNd6Q8c5PwLMNqjsB5myjXq1WBbHEO3yiNpz89jk3hu8ETz\nq3gMylQikJ7mLPL2P8+Lo7kD4egJ9IGJ2PHLwjxTu1WLgAciEOpGWjqv7E1/qzu9\nd//QIUBQKd+HQ3RaWEV+FcAu\n-----END PRIVATE KEY-----\n",
    "client_email": "firebase-adminsdk-fbsvc@examplecpm-41336.iam.gserviceaccount.com",
    "client_id": "102242565442763077385",
    "auth_uri": "https://accounts.google.com/o/oauth2/auth",
    "token_uri": "https://oauth2.googleapis.com/token",
    "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
    "client_x509_cert_url": "https://www.googleapis.com/robot/v1/metadata/x509/firebase-adminsdk-fbsvc%40examplecpm-41336.iam.gserviceaccount.com",
    "universe_domain": "googleapis.com"
  }),
  databaseURL: "https://examplecpm-41336-default-rtdb.europe-west1.firebasedatabase.app/"
});


const db = admin.database();
const daysToCalculate = 2;

async function shouldRunScript() {
  const initDateSnap = await db.ref("Data/1_InitDate").once("value");

  if (!initDateSnap.exists()) {
    return false;
  }

  const initDateStr = initDateSnap.val();
  const initDate = new Date(initDateStr);
  if (isNaN(initDate)) {
    return false;
  }

  const now = new Date();
  const diffDays = (now - initDate) / (1000 * 60 * 60 * 24);

  return diffDays >= daysToCalculate;
}

async function calculateAvgEcpm() {
  const dataRef = db.ref("Data");
  const snapshot = await dataRef.once("value");
  const data = snapshot.val();

  for (const country in data) {
    if (!data[country].Users) continue;

    const users = data[country].Users;
    let totalEcpm = 0;
    let count = 0;

    for (const uid in users) {
      const ecpm = parseFloat(users[uid].ecpm);
      if (!isNaN(ecpm)) {
        totalEcpm += ecpm;
        count++;
      }
    }

    if (count > 0) {
      const avgEcpm = totalEcpm / count;
      await dataRef.child(country).child("AvgEcpm").set(avgEcpm.toFixed(2));
      console.log(`[${country}] AvgEcpm: ${avgEcpm.toFixed(2)}`);
    }
  }
}

async function findAllMoreAvgEcpm() {
  const dataRef = db.ref("Data");
  const snapshot = await dataRef.once("value");
  const data = snapshot.val();

  for (const country in data) {
    const avg = parseFloat(data[country].AvgEcpm || 0);
    if (!data[country].Users || isNaN(avg)) continue;

    const users = data[country].Users;
    const aboveAvgUsers = [];

    for (const uid in users) {
      const ecpm = parseFloat(users[uid].ecpm);
      if (!isNaN(ecpm) && ecpm > avg) {
        aboveAvgUsers.push(users[uid]);
      }
    }

    if (aboveAvgUsers.length > 0) {
      await dataRef.child(country).child("AboveAvgUsers").set(aboveAvgUsers);
    }
  }
}

(async () => {
  const shouldRun = await shouldRunScript();

  if (shouldRun) {
    console.log("Идет подсчет...");
    await calculateAvgEcpm();
    await findAllMoreAvgEcpm();
  } else {
    console.log("Рано");
  }
})();
