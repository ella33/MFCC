<%--
  Created by IntelliJ IDEA.
  User: Elev
  Date: 05.02.2017
  Time: 21:15
  To change this template use File | Settings | File Templates.
--%>
<%@ page contentType="text/html;charset=UTF-8" language="java" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<html>
<head>
    <title>Books</title>
</head>
<body>
List of books
<table>
    <c:forEach items="${requestScope.books}" var="book">
        <tr>
            <td>${book.id}</td>
            <td>${book.title}</td>
            <td>${book.price}</td>
            <td>${book.copies}</td>
        </tr>
    </c:forEach>
</table>

</body>
</html>
