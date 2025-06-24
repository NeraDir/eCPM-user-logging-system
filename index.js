const admin = require("firebase-admin");

admin.initializeApp({
  credential: admin.credential.cert({
      "type": "service_account",
      "project_id": "examplecpm-41336",
      "private_key_id": "96d12cdd9ad8d441b15e7f836387cc004850c1f2",
      "private_key": "-----BEGIN PRIVATE KEY-----\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQDHwzYZptZg8Fjn\n+v7iPRcyrWDNZXua5T8+Mlhgjc0Zfa5i0LuXSL+/uoTn5BWC1r8apt+M4pajn7BD\ncgTEC6w4+Ge9AdFDE+dEGu6M7+xG80V5oAVFvlCUSALIv+SBj/IqgNflDDHtXLcP\ncYMNfxJhk/y/3VPpePIv389839lAbTpUp5bLXyF/Gz6W97bC8bWvwFcIJ2rECWGm\nO7qVKqhS6meF9LqgDc+OL3g83e8cycBmLtoAwo4idWCAfqySMxGnU37MuG4TufNy\nzedGicdEPZ0km/1ZOS3+pZyXnW44ONHtmpmZ0S4aT3fUfHk6GgdgXd+ncuuaRV2L\nUM9aXdcLAgMBAAECggEAGTn64i4STYdHRZ20mV/AnGA5D3HSNFiLQ4X6aNBkxw7Q\nOl9aCTv2fGEIDqaxhrvVbKldJWlUlPuCDV+1NV1fVYnHw/PE+D+9CU84sn32lxZ6\nlBDpXt+D79h7gtwkywMhavepYUidzM1UEB8JaedoVqqTFQGqFUZLmKXJXzTU4SXm\nGyoon9MZrM6/BgBXJGi+WEI0g6RmRrLQUR1EkdoUiL5ZjZsMOzqOW39hZdVjH/ZN\ngJMSXRGUsNhB3ki3jOxfbDv+cyPlt8OimQ12/Lhc5dRoahBBGgIv+1q2HbbaDhzF\nYxCKFUkXtiKcXOWgPb3KmY/YAOESKYATaAv4SPTzzQKBgQD0EAQFGhX3saKI2TZX\n1QzPAQapB5M/g6qVZxQsZlTBd6Z7bQS/RBtu0Cw+1EErjx1HbcpY87Dn4zXQ9Q3O\noUFcn4XbqK1uPndUBs+JvpR+kIB0tnEFsF3uUmMsjvJooiiNN50kjDO/Bg8NirTw\nO6Ml9xjGi1zWcQueXVXPEqsQzQKBgQDRiIBK1SEXNHUh1iEE326FWWHAbATvTpYF\nil5h1oOsDK7fF4BJHTkOKCIzb/CLhj9Ahym6zWGbM5ALe8qttVm7UmYZ/QrNfGQ1\nhQlUX+UwwS9YzdSr+yMv3tKRLhTfr18YDhem+QzI5N0kvfiunzkfx7vB0o2P6HjT\nWUjCcEYnNwKBgF9xh/Dq055q1DIKJ8tPYNPvBcjCUp4rAmGszuHSHuENxohMCOg3\nXTcHp9XmMZo0PHH7iH8ixZLZculFe+HLhAERzUoqe7A6M3Yjt7OZWP7pyaJa2nYs\nE3+ormn1eOzcRVl8XzK0tWPFnctg7ANqfxHQwNqcE3E1AiMrQuxY05cdAoGAdcit\nlWNJPqoWTfb4iKywi/U6pdgSzhL28hZeB9F2IvjeNDoZuv/aWsLkttVt+oB0fel8\n460f+QZ7Knlj7VHMJyiijGlZ8TjDe/JU1EEzrLxryRerPSHnHbm71DB7O2JxqiwB\nz7KgJS3Bwi2n5UFL8zlrqgCZ4xNjGCtQYzyJsvECgYEAuOKmppJBx375C34CyWGX\nlo66l9St72N5oxGZQLN9yVXcmOQ6GIC75VFz7+PE0N2Zj1uT4Nz65ktD9av9YtEw\nPFzf9FEx/tTBfqkwYRN86E5R6XEvfSmxmXevgKWdUmOxRycEDung7dlp4ZvQ0jM2\nAvTUcvBK0jo1/8pnmHnECOI=\n-----END PRIVATE KEY-----\n",
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
      console.log([${country}] AvgEcpm: ${avgEcpm.toFixed(2)});
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
      console.log([${country}] AboveAvgUsers: ${aboveAvgUsers.length});
    }
  }
}

(async () => {
  await calculateAvgEcpm();
  await findAllMoreAvgEcpm();
})();
