<%--
  Created by IntelliJ IDEA.
  User: Elev
  Date: 04.02.2017
  Time: 22:07
  To change this template use File | Settings | File Templates.
--%>

<%@ page contentType="text/html;charset=UTF-8" language="java" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<html>
<head>
    <title>Users</title>
</head>
<body>
List of users
<table>
    <c:forEach items="${requestScope.users}" var="user">
        <tr>
            <td>${user.id}</td>
            <td>${user.username}</td>
        </tr>
    </c:forEach>
</table>
</body>
</html>
