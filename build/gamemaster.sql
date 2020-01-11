/*
Navicat MySQL Data Transfer

Source Server         : localhost
Source Server Version : 50505
Source Host           : localhost:3306
Source Database       : gamemaster2

Target Server Type    : MYSQL
Target Server Version : 50505
File Encoding         : 65001

Date: 2020-01-11 23:59:53
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for `gameserver`
-- ----------------------------
DROP TABLE IF EXISTS `gameserver`;
CREATE TABLE `gameserver` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `masterserver` int(11) NOT NULL,
  `dynamic` int(1) NOT NULL,
  `challengeok` int(1) NOT NULL,
  `handshakeok` int(1) NOT NULL,
  `address` varchar(100) DEFAULT NULL,
  `port` int(11) DEFAULT NULL,
  `hostport` int(11) DEFAULT NULL,
  `protocol` varchar(50) DEFAULT 'gamespy2',
  `type` varchar(50) DEFAULT 'swbf2',
  `gq_hostname` varchar(255) DEFAULT NULL,
  `gq_gametype` varchar(255) DEFAULT '0',
  `gq_mapname` varchar(255) DEFAULT NULL,
  `gq_maxplayers` int(11) DEFAULT '0',
  `gq_numplayers` int(11) DEFAULT '0',
  `gq_password` int(11) DEFAULT '0',
  `team0_name` varchar(50) DEFAULT NULL,
  `team0_score` int(11) DEFAULT '0',
  `team1_name` varchar(50) DEFAULT NULL,
  `team1_score` int(11) DEFAULT '0',
  `gamever` varchar(50) NOT NULL,
  `session` int(11) NOT NULL,
  `prevsession` int(11) NOT NULL,
  `servertype` int(1) NOT NULL,
  `gamemode` varchar(50) NOT NULL,
  `localips` varchar(255) NOT NULL,
  `clientid` int(11) NOT NULL,
  `netregion` int(1) NOT NULL,
  `custom` text NOT NULL,
  `lastseen` int(11) DEFAULT '0',
  `lastquery` int(11) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of gameserver
-- ----------------------------

-- ----------------------------
-- Table structure for `masterserver`
-- ----------------------------
DROP TABLE IF EXISTS `masterserver`;
CREATE TABLE `masterserver` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `server_name` varchar(32) DEFAULT NULL,
  `server_address` varchar(15) DEFAULT NULL,
  `server_port` int(11) DEFAULT NULL,
  `server_natnegaddress` varchar(100) NOT NULL,
  `server_natnegport` int(11) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records of masterserver
-- ----------------------------

-- ----------------------------
-- Table structure for `natneg`
-- ----------------------------
DROP TABLE IF EXISTS `natneg`;
CREATE TABLE `natneg` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `natneg_masterserver` int(11) NOT NULL,
  `natneg_cookie` int(11) NOT NULL DEFAULT '0',
  `natneg_gamename` varchar(50) DEFAULT NULL,
  `natneg_sequence` int(1) DEFAULT NULL,
  `natneg_localip` varchar(15) DEFAULT NULL,
  `natneg_localport` int(11) DEFAULT NULL,
  `natneg_remoteip` varchar(15) DEFAULT NULL,
  `natneg_remoteport` int(11) DEFAULT NULL,
  `natneg_clienttype` int(1) DEFAULT NULL,
  `natneg_comport` int(11) DEFAULT NULL,
  `natneg_lastupdate` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records of natneg
-- ----------------------------

-- ----------------------------
-- Table structure for `players`
-- ----------------------------
DROP TABLE IF EXISTS `players`;
CREATE TABLE `players` (
  `id` int(10) NOT NULL AUTO_INCREMENT,
  `sid` int(10) DEFAULT NULL,
  `gq_name` varchar(255) DEFAULT NULL,
  `gq_team` int(11) NOT NULL,
  `gq_kills` int(11) DEFAULT NULL,
  `gq_deaths` int(11) DEFAULT NULL,
  `gq_score` int(11) DEFAULT NULL,
  `gq_ping` int(11) unsigned DEFAULT NULL,
  `lastseen` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `gq_name` (`gq_name`),
  KEY `lastseen` (`lastseen`),
  KEY `sid` (`sid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of players
-- ----------------------------

-- ----------------------------
-- Table structure for `serverkeys`
-- ----------------------------
DROP TABLE IF EXISTS `serverkeys`;
CREATE TABLE `serverkeys` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `key_gamename` varchar(20) DEFAULT NULL,
  `key_key` varchar(6) DEFAULT NULL,
  `supported` int(1) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records of serverkeys
-- ----------------------------

-- ----------------------------
-- Table structure for `stats_rounds`
-- ----------------------------
DROP TABLE IF EXISTS `stats_rounds`;
CREATE TABLE `stats_rounds` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `sid` int(11) NOT NULL,
  `uid` int(11) NOT NULL,
  `player` varchar(100) NOT NULL,
  `auth` varchar(255) NOT NULL,
  `pid` int(11) NOT NULL,
  `finishes` int(1) NOT NULL,
  `deaths` int(11) NOT NULL,
  `kills` int(11) NOT NULL,
  `endfaction` varchar(20) NOT NULL,
  `playerpoints` int(11) NOT NULL,
  `timePlayed` int(11) NOT NULL,
  `ctime` int(11) NOT NULL,
  `dtime` int(11) NOT NULL,
  `starts` int(11) NOT NULL,
  `heropoints` int(11) NOT NULL,
  `livingStreak` int(11) NOT NULL,
  `rating` int(11) NOT NULL,
  `mapname` varchar(50) NOT NULL,
  `winningTeam` varchar(20) NOT NULL,
  `gameComplete` int(1) NOT NULL,
  `winningCnt` int(11) NOT NULL,
  `losingCnt` int(11) NOT NULL,
  `gametype` varchar(50) NOT NULL,
  `losingTeam` varchar(50) NOT NULL,
  `GameMode` int(1) NOT NULL,
  `timestamp` int(11) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records of stats_rounds
-- ----------------------------

-- ----------------------------
-- Table structure for `stats_servers`
-- ----------------------------
DROP TABLE IF EXISTS `stats_servers`;
CREATE TABLE `stats_servers` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `title` varchar(255) NOT NULL,
  `desc` text NOT NULL,
  `ip` varchar(100) NOT NULL,
  `active` int(1) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records of stats_servers
-- ----------------------------

-- ----------------------------
-- Table structure for `users`
-- ----------------------------
DROP TABLE IF EXISTS `users`;
CREATE TABLE `users` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  `password` varchar(255) NOT NULL,
  `email` varchar(50) NOT NULL,
  `country` varchar(4) NOT NULL,
  `session` int(11) DEFAULT '0',
  `game` varchar(255) NOT NULL,
  `status` int(1) NOT NULL,
  `status_string` varchar(255) NOT NULL,
  `stats_finishes` int(11) NOT NULL,
  `stats_deaths` int(11) NOT NULL,
  `stats_kills` int(11) NOT NULL,
  `stats_playerpoints` int(11) NOT NULL,
  `stats_timePlayed` int(11) NOT NULL,
  `stats_ctime` int(11) NOT NULL,
  `stats_starts` int(11) NOT NULL,
  `stats_heropoints` int(11) NOT NULL,
  `stats_livingStreak` int(11) NOT NULL,
  `stats_rating` int(11) NOT NULL,
  `stats_gameComplete` int(11) NOT NULL,
  `stats_winningCnt` int(11) NOT NULL,
  `stats_losingCnt` int(11) NOT NULL,
  `stats_roundsplayed` int(11) NOT NULL,
  `stats_lastPlayed` int(11) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records of users
-- ----------------------------
