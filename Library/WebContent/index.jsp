<%--
  Created by IntelliJ IDEA.
  User: Elev
  Date: 04.02.2017
  Time: 15:39
  To change this template use File | Settings | File Templates.
--%>
<%@ page contentType="text/html;charset=UTF-8" language="java" %>
<html>
  <head>
    <title>Online library</title>
  </head>
  <p>Online library</p>
  <form action="/Library_war_exploded/library" method="post">
    <input type="submit" name="startTransaction" value="Start transaction">
    <p>
    Transaction ID: <input type="text" name="transactionID" />
    </p>
    <p>
    Username: <input type="text" name="username"/>
    <input type="submit" name="addUser" value="Add new user">
    </p>

    <p>
    <input type="submit" name="listBooks" value="List books">
    <input type="submit" name="listUsers" value="List users">
    </p>


    <p>
    Title: <input type="text" name="title"/>
    Price: <input type="text" name="price"/>
    Copies: <input type="text" name="copies"/>
    <input type="submit" name="addBook" value="Add new book">
    </p>

    <p>
    User ID: <input type="text" name="userID"/>
    <input type="submit" name="deleteUser" value="Delete user">
    </p>

    <p>
    Book ID: <input type="text" name="bookIDUpdate"/>
    Copies: <input type="text" name="copiesUpdate"/>
    <input type="submit" name="buyBook" value="Update copies for book">
    </p>

    <p>
    Book ID: <input type="text" name="bookIDBorrow"/>
    User ID: <input type="text" name="userIDBorrow"/>
    <input type="submit" name="borrowBook" value="Borrow book">
    <input type="submit" name="returnBook" value="Return book">
    </p>

    <p>
    <input type="submit" name="commitTransaction" value="Commit transaction">
    <input type="submit" name="abortTransaction" value="Abort transaction">
    </p>

  </form>
  </body>
</html>
