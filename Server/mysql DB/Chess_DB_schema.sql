-- MariaDB dump 10.19  Distrib 10.5.15-MariaDB, for debian-linux-gnueabihf (armv8l)
--
-- Host: localhost    Database: Chess_DB
-- ------------------------------------------------------
-- Server version	10.5.15-MariaDB-0+deb11u1

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `Game_History`
--

DROP TABLE IF EXISTS `Game_History`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Game_History` (
  `White_Nickname` varchar(30) DEFAULT NULL,
  `White_Rating` int(11) DEFAULT NULL,
  `Black_Nickname` varchar(30) DEFAULT NULL,
  `Black_Rating` int(11) DEFAULT NULL,
  `Winner_Nickname` varchar(30) DEFAULT NULL,
  `Friendly_Game` tinyint(1) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Ongoing_Games`
--

DROP TABLE IF EXISTS `Ongoing_Games`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Ongoing_Games` (
  `White_Nickname` varchar(30) DEFAULT NULL,
  `White_Rating` int(11) DEFAULT NULL,
  `Black_Nickname` varchar(30) DEFAULT NULL,
  `Black_Rating` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Online_Users`
--

DROP TABLE IF EXISTS `Online_Users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Online_Users` (
  `Socket` int(11) DEFAULT NULL,
  `Pipe_Write` int(11) DEFAULT NULL,
  `Pipe_Read` int(11) DEFAULT NULL,
  `State` varchar(5) DEFAULT NULL,
  `Username` varchar(30) DEFAULT NULL,
  UNIQUE KEY `Username` (`Username`),
  CONSTRAINT `Online_Users_ibfk_1` FOREIGN KEY (`Username`) REFERENCES `Users` (`Username`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Temporary table structure for view `Online_Users_Server`
--

DROP TABLE IF EXISTS `Online_Users_Server`;
/*!50001 DROP VIEW IF EXISTS `Online_Users_Server`*/;
SET @saved_cs_client     = @@character_set_client;
SET character_set_client = utf8;
/*!50001 CREATE TABLE `Online_Users_Server` (
  `Socket` tinyint NOT NULL,
  `Pipe_Write` tinyint NOT NULL,
  `Pipe_Read` tinyint NOT NULL,
  `State` tinyint NOT NULL,
  `Username` tinyint NOT NULL,
  `Rating` tinyint NOT NULL,
  `Game_Rating_Delta` tinyint NOT NULL,
  `Profile_Pic` tinyint NOT NULL,
  `Nickname` tinyint NOT NULL
) ENGINE=MyISAM */;
SET character_set_client = @saved_cs_client;

--
-- Table structure for table `Users`
--

DROP TABLE IF EXISTS `Users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Users` (
  `Nickname` varchar(30) NOT NULL,
  `Username` varchar(30) NOT NULL,
  `Password` varchar(30) DEFAULT NULL,
  `Profile_Pic` int(11) DEFAULT NULL,
  `Rating` int(11) DEFAULT NULL,
  `Game_Rating_Delta` int(11) DEFAULT NULL,
  PRIMARY KEY (`Username`),
  UNIQUE KEY `Nickname` (`Nickname`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Final view structure for view `Online_Users_Server`
--

/*!50001 DROP TABLE IF EXISTS `Online_Users_Server`*/;
/*!50001 DROP VIEW IF EXISTS `Online_Users_Server`*/;
/*!50001 SET @saved_cs_client          = @@character_set_client */;
/*!50001 SET @saved_cs_results         = @@character_set_results */;
/*!50001 SET @saved_col_connection     = @@collation_connection */;
/*!50001 SET character_set_client      = utf8 */;
/*!50001 SET character_set_results     = utf8 */;
/*!50001 SET collation_connection      = utf8_general_ci */;
/*!50001 CREATE ALGORITHM=UNDEFINED */
/*!50013 DEFINER=`yakir`@`localhost` SQL SECURITY DEFINER */
/*!50001 VIEW `Online_Users_Server` AS select `Online_Users`.`Socket` AS `Socket`,`Online_Users`.`Pipe_Write` AS `Pipe_Write`,`Online_Users`.`Pipe_Read` AS `Pipe_Read`,`Online_Users`.`State` AS `State`,`Users`.`Username` AS `Username`,`Users`.`Rating` AS `Rating`,`Users`.`Game_Rating_Delta` AS `Game_Rating_Delta`,`Users`.`Profile_Pic` AS `Profile_Pic`,`Users`.`Nickname` AS `Nickname` from (`Online_Users` join `Users` on(`Online_Users`.`Username` = `Users`.`Username`)) */;
/*!50001 SET character_set_client      = @saved_cs_client */;
/*!50001 SET character_set_results     = @saved_cs_results */;
/*!50001 SET collation_connection      = @saved_col_connection */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2022-09-20 22:10:14
